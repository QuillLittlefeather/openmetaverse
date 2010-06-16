﻿/*
 * Copyright (c) 2010 Open Metaverse Foundation
 * All rights reserved.
 *
 * - Redistribution and use in source and binary forms, with or without 
 *   modification, are permitted provided that the following conditions are met:
 *
 * - Redistributions of source code must retain the above copyright notice, this
 *   list of conditions and the following disclaimer.
 * - Neither the name of the openmetaverse.org nor the names 
 *   of its contributors may be used to endorse or promote products derived from
 *   this software without specific prior written permission.
 *
 * THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" 
 * AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE 
 * IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE 
 * ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE 
 * LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR 
 * CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF 
 * SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS 
 * INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN 
 * CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) 
 * ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE 
 * POSSIBILITY OF SUCH DAMAGE.
 */

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.IO;
using System.Net;
using MySql.Data.MySqlClient;
using OpenSimDB;
using OpenMetaverse;
using OpenMetaverse.StructuredData;

namespace RobustMigration
{
    public class AssetMigration
    {
        private MySqlConnection m_connection;
        private opensim m_db;
        private string m_assetUrl;

        public AssetMigration(string connectString, string assetServiceUrl)
        {
            using (m_connection = new MySqlConnection(connectString))
            {
                using (m_db = new opensim(m_connection))
                {
                    m_assetUrl = assetServiceUrl;

                    var assets = from a in m_db.assets
                                 select a;

                    int i = 0;
                    foreach (var asset in assets)
                    {
                        CreateAsset(asset);
                        if (++i % 100 == 0) Console.Write(".");
                    }
                }
            }
        }

        private void CreateAsset(assets asset)
        {
            AssetType type = (AssetType)asset.assetType;
            string contentType = LLUtil.SLAssetTypeToContentType((int)type);
            bool isPublic = true;
            string errorMessage = null;

            // Distinguish public and private assets
            switch (type)
            {
                case AssetType.CallingCard:
                case AssetType.Gesture:
                case AssetType.LSLBytecode:
                case AssetType.LSLText:
                    isPublic = false;
                    break;
            }

            // Build the remote storage request
            List<MultipartForm.Element> postParameters = new List<MultipartForm.Element>()
            {
                new MultipartForm.Parameter("AssetID", asset.id),
                new MultipartForm.Parameter("CreatorID", asset.CreatorID),
                new MultipartForm.Parameter("Temporary", asset.temporary.ToString()),
                new MultipartForm.Parameter("Public", isPublic ? "1" : "0"),
                new MultipartForm.File("Asset", asset.name, contentType, asset.data)
            };

            // Make the remote storage request
            try
            {
                HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(m_assetUrl);

                HttpWebResponse response = MultipartForm.Post(request, postParameters);
                using (Stream responseStream = response.GetResponseStream())
                {
                    string responseStr = null;

                    try
                    {
                        responseStr = responseStream.GetStreamString();
                        OSD responseOSD = OSDParser.Deserialize(responseStr);
                        if (responseOSD.Type == OSDType.Map)
                        {
                            OSDMap responseMap = (OSDMap)responseOSD;
                            if (responseMap["Success"].AsBoolean())
                                return;
                            else
                                errorMessage = "Upload failed: " + responseMap["Message"].AsString();
                        }
                        else
                        {
                            errorMessage = "Response format was invalid:\n" + responseStr;
                        }
                    }
                    catch (Exception ex)
                    {
                        if (!String.IsNullOrEmpty(responseStr))
                            errorMessage = "Failed to parse the response:\n" + responseStr;
                        else
                            errorMessage = "Failed to retrieve the response: " + ex.Message;
                    }
                }
            }
            catch (WebException ex)
            {
                errorMessage = ex.Message;
            }

            Console.WriteLine("Failed to store asset \"{0}\" ({1}, {2}): {3}", asset.name, asset.id, contentType, errorMessage);
        }
    }
}
