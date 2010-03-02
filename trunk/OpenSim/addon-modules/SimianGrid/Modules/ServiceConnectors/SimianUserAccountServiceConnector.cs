﻿/*
 * Copyright (c) Contributors, http://opensimulator.org/
 * See CONTRIBUTORS.TXT for a full list of copyright holders.
 *
 * Redistribution and use in source and binary forms, with or without
 * modification, are permitted provided that the following conditions are met:
 *     * Redistributions of source code must retain the above copyright
 *       notice, this list of conditions and the following disclaimer.
 *     * Redistributions in binary form must reproduce the above copyright
 *       notice, this list of conditions and the following disclaimer in the
 *       documentation and/or other materials provided with the distribution.
 *     * Neither the name of the OpenSimulator Project nor the
 *       names of its contributors may be used to endorse or promote products
 *       derived from this software without specific prior written permission.
 *
 * THIS SOFTWARE IS PROVIDED BY THE DEVELOPERS ``AS IS'' AND ANY
 * EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
 * WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
 * DISCLAIMED. IN NO EVENT SHALL THE CONTRIBUTORS BE LIABLE FOR ANY
 * DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
 * (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
 * LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND
 * ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
 * (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
 * SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 */

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Reflection;
using OpenSim.Region.Framework.Interfaces;
using OpenSim.Region.Framework.Scenes;
using OpenSim.Services.Interfaces;
using log4net;
using Mono.Addins;
using Nini.Config;
using OpenMetaverse;
using OpenMetaverse.StructuredData;

namespace SimianGrid
{
    [Extension(Path = "/OpenSim/RegionModules", NodeName = "RegionModule")]
    public class SimianUserAccountServiceConnector : IUserAccountService, ISharedRegionModule
    {
        private static readonly ILog m_log =
                LogManager.GetLogger(
                MethodBase.GetCurrentMethod().DeclaringType);

        private string m_serverUrl = String.Empty;

        #region ISharedRegionModule

        public Type ReplaceableInterface { get { return null; } }
        public void RegionLoaded(Scene scene) { }
        public void PostInitialise() { }
        public void Close() { }

        public SimianUserAccountServiceConnector() { }
        public string Name { get { return "SimianUserAccountServiceConnector"; } }
        public void AddRegion(Scene scene) { scene.RegisterModuleInterface<IUserAccountService>(this); }
        public void RemoveRegion(Scene scene) { scene.UnregisterModuleInterface<IUserAccountService>(this); }

        #endregion ISharedRegionModule

        public SimianUserAccountServiceConnector(IConfigSource source)
        {
            Initialise(source);
        }

        public void Initialise(IConfigSource source)
        {
            IConfig assetConfig = source.Configs["UserAccountService"];
            if (assetConfig == null)
            {
                m_log.Error("[ACCOUNT CONNECTOR]: UserAccountService missing from OpenSim.ini");
                throw new Exception("User account connector init error");
            }

            string serviceURI = assetConfig.GetString("UserAccountServerURI");
            if (String.IsNullOrEmpty(serviceURI))
            {
                m_log.Error("[ACCOUNT CONNECTOR]: No UserAccountServerURI in section UserAccountService");
                throw new Exception("User account connector init error");
            }

            m_serverUrl = serviceURI;
        }

        public UserAccount GetUserAccount(UUID scopeID, string firstName, string lastName)
        {
            NameValueCollection requestArgs = new NameValueCollection
            {
                { "RequestMethod", "GetUser" },
                { "Name", firstName + ' ' + lastName }
            };

            return GetUser(requestArgs);
        }

        public UserAccount GetUserAccount(UUID scopeID, string email)
        {
            NameValueCollection requestArgs = new NameValueCollection
            {
                { "RequestMethod", "GetUser" },
                { "Email", email }
            };

            return GetUser(requestArgs);
        }

        public UserAccount GetUserAccount(UUID scopeID, UUID userID)
        {
            NameValueCollection requestArgs = new NameValueCollection
            {
                { "RequestMethod", "GetUser" },
                { "UserID", userID.ToString() }
            };

            return GetUser(requestArgs);
        }

        public List<UserAccount> GetUserAccounts(UUID scopeID, string query)
        {
            List<UserAccount> accounts = new List<UserAccount>();

            m_log.InfoFormat("[ACCOUNT CONNECTOR]: Searching for user accounts with name query " + query);

            NameValueCollection requestArgs = new NameValueCollection
            {
                { "RequestMethod", "GetUsers" },
                { "NameQuery", query }
            };

            OSDMap response = WebUtil.PostToService(m_serverUrl, requestArgs);
            if (response["Success"].AsBoolean())
            {
                OSDArray array = response["Users"] as OSDArray;
                if (array != null && array.Count > 0)
                {
                    for (int i = 0; i < array.Count; i++)
                    {
                        UserAccount account = ResponseToUserAccount(array[i] as OSDMap);
                        if (account != null)
                            accounts.Add(account);
                    }
                }
                else
                {
                    m_log.Warn("[ACCOUNT CONNECTOR]: Account search failed, response data was in an invalid format");
                }
            }
            else
            {
                m_log.Warn("[ACCOUNT CONNECTOR]: Failed to search for account data by name " + query);
            }

            return accounts;
        }

        public bool StoreUserAccount(UserAccount data)
        {
            m_log.InfoFormat("[ACCOUNT CONNECTOR]: Storing user account for " + data.Name);

            NameValueCollection requestArgs = new NameValueCollection
            {
                { "RequestMethod", "AddUser" },
                { "UserID", data.PrincipalID.ToString() },
                { "Name", data.Name },
                { "Email", data.Email },
                { "AccessLevel", data.UserLevel.ToString() }
            };

            OSDMap response = WebUtil.PostToService(m_serverUrl, requestArgs);
            
            if (response["Success"].AsBoolean())
            {
                m_log.InfoFormat("[ACCOUNT CONNECTOR]: Storing user account data for " + data.Name);

                requestArgs = new NameValueCollection
                {
                    { "RequestMethod", "AddUserData" },
                    { "UserID", data.PrincipalID.ToString() },
                    { "CreationDate", data.Created.ToString() },
                    { "UserFlags", data.UserFlags.ToString() },
                    { "UserTitle", data.UserTitle }
                };

                response = WebUtil.PostToService(m_serverUrl, requestArgs);
                bool success = response["Success"].AsBoolean();

                if (!success)
                    m_log.Warn("[ACCOUNT CONNECTOR]: Failed to store user account data for " + data.Name + ": " + response["Message"].AsString());

                return success;
            }
            else
            {
                m_log.Warn("[ACCOUNT CONNECTOR]: Failed to store user account for " + data.Name + ": " + response["Message"].AsString());
            }

            return false;
        }

        /// <summary>
        /// Helper method for the various ways of retrieving a user account
        /// </summary>
        /// <param name="requestArgs">Service query parameters</param>
        /// <returns>A UserAccount object on success, null on failure</returns>
        private UserAccount GetUser(NameValueCollection requestArgs)
        {
            m_log.InfoFormat("[ACCOUNT CONNECTOR]: Looking up user account");

            OSDMap response = WebUtil.PostToService(m_serverUrl, requestArgs);
            if (response["Success"].AsBoolean())
            {
                OSDMap user = response["User"] as OSDMap;
                if (user != null)
                    return ResponseToUserAccount(user);
                else
                    m_log.Warn("[ACCOUNT CONNECTOR]: Account search failed, response data was in an invalid format");
            }
            else
            {
                string lookupValue = (requestArgs.Count > 1) ? requestArgs[1] : "(Unknown)";
                m_log.Warn("[ACCOUNT CONNECTOR]: Failed to lookup user account with query: " + lookupValue);
            }

            return null;
        }

        /// <summary>
        /// Convert a User object in LLSD format to a UserAccount
        /// </summary>
        /// <param name="response">LLSD containing user account data</param>
        /// <returns>A UserAccount object on success, null on failure</returns>
        private UserAccount ResponseToUserAccount(OSDMap response)
        {
            if (response == null)
                return null;

            UserAccount account = new UserAccount();
            account.PrincipalID = response["UserID"].AsUUID();
            account.Created = response["CreationDate"].AsInteger();
            account.Email = response["Email"].AsString();
            account.ServiceURLs = new Dictionary<string, object>(0);
            account.UserFlags = response["UserFlags"].AsInteger();
            account.UserLevel = response["AccessLevel"].AsInteger();
            account.UserTitle = response["UserTitle"].AsString();
            GetFirstLastName(response["Name"].AsString(), out account.FirstName, out account.LastName);

            return account;
        }

        /// <summary>
        /// Convert a name with a single space in it to a first and last name
        /// </summary>
        /// <param name="name">A full name such as "John Doe"</param>
        /// <param name="firstName">First name</param>
        /// <param name="lastName">Last name (surname)</param>
        private static void GetFirstLastName(string name, out string firstName, out string lastName)
        {
            if (String.IsNullOrEmpty(name))
            {
                firstName = String.Empty;
                lastName = String.Empty;
            }
            else
            {
                string[] names = name.Split(' ');

                if (names.Length == 2)
                {
                    firstName = names[0];
                    lastName = names[1];
                }
                else
                {
                    firstName = String.Empty;
                    lastName = name;
                }
            }
        }
    }
}
