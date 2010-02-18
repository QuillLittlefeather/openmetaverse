<?php
/** Simian grid services
 *
 * PHP version 5
 *
 * Redistribution and use in source and binary forms, with or without
 * modification, are permitted provided that the following conditions
 * are met:
 * 1. Redistributions of source code must retain the above copyright
 *    notice, this list of conditions and the following disclaimer.
 * 2. Redistributions in binary form must reproduce the above copyright
 *    notice, this list of conditions and the following disclaimer in the
 *    documentation and/or other materials provided with the distribution.
 * 3. The name of the author may not be used to endorse or promote products
 *    derived from this software without specific prior written permission.
 * 
 * THIS SOFTWARE IS PROVIDED BY THE AUTHOR ``AS IS'' AND ANY EXPRESS OR
 * IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES
 * OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED.
 * IN NO EVENT SHALL THE AUTHOR BE LIABLE FOR ANY DIRECT, INDIRECT,
 * INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT
 * NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE,
 * DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY
 * THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
 * (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF
 * THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 *
 *
 * @package    SimianGrid
 * @author     Jim Radford <http://www.jimradford.com/>
 * @copyright  Open Metaverse Foundation
 * @license    http://www.debian.org/misc/bsd.license  BSD License (3 Clause)
 * @link       http://openmetaverse.googlecode.com/
 */
interface_exists('IGridService') || require_once ('Interface.GridService.php');
class_exists('UUID') || require_once ('Class.UUID.php');

class GetIdentities implements IGridService
{
    private $UserID;
    
    public function Execute($db, $params, $logger)
    {
        if (isset($params["UserID"]) && UUID::TryParse($params["UserID"], $this->UserID))
        {
            $sql = "SELECT Identifier, Type, Credential, Enabled FROM Identities WHERE UserID=:UserID";
            
            $sth = $db->prepare($sql);
            
            if ($sth->execute(array(':UserID' => $this->UserID)))
            {
                $this->HandleQueryResponse($sth);
            }
            else
            {
                $logger->err(sprintf("Error occurred during query: %d %s", $sth->errorCode(), print_r($sth->errorInfo(), true)));
                $logger->debug(sprintf("Query: %s", $sql));
                header("Content-Type: application/json", true);
                echo '{ "Message": "Database query error" }';
                exit();
            }
        }
        else
        {
            header("Content-Type: application/json", true);
            echo '{ "Message": "Invalid parameters" }';
            exit();
        }
    }

    private function HandleQueryResponse($sth)
    {
        if ($sth->rowCount() > 0)
        {
            $found = array();
            
            while ($obj = $sth->fetchObject())
            {
                $found[] = sprintf('{"Identifier":"%s","Credential":"%s","Type":"%s","Enabled":%s}',
                    $obj->Identifier, $obj->Credential, $obj->Type, ($obj->Enabled) ? 'true' : 'false');
            }
            
            header("Content-Type: application/json", true);
            echo '{"Success":true,"Identities":[' . implode(',', $found) . ']}';
            exit();
        }
        else
        {
            header("Content-Type: application/json", true);
            echo '{"Success:true,"Identities":[]}';
            exit();
        }
    }
}
