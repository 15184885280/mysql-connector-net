// Copyright © 2013, 2018, Oracle and/or its affiliates. All rights reserved.
//
// MySQL Connector/NET is licensed under the terms of the GPLv2
// <http://www.gnu.org/licenses/old-licenses/gpl-2.0.html>, like most 
// MySQL Connectors. There are special exceptions to the terms and 
// conditions of the GPLv2 as it is applied to this software, see the 
// FLOSS License Exception
// <http://www.mysql.com/about/legal/licensing/foss-exception.html>.
//
// This program is free software; you can redistribute it and/or modify 
// it under the terms of the GNU General Public License as published 
// by the Free Software Foundation; version 2 of the License.
//
// This program is distributed in the hope that it will be useful, but 
// WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY 
// or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License 
// for more details.
//
// You should have received a copy of the GNU General Public License along 
// with this program; if not, write to the Free Software Foundation, Inc., 
// 51 Franklin St, Fifth Floor, Boston, MA 02110-1301  USA

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Security.Cryptography;
#if NETSTANDARD1_3
using AliasText = MySql.Data.MySqlClient.Framework.NetStandard1_3;
#else
using AliasText = System.Text;
#endif

namespace MySql.Data.MySqlClient.Authentication
{
  /// <summary>
  /// The implementation of the sha256_password authentication plugin.
  /// </summary>
  public class Sha256AuthenticationPlugin : MySqlAuthenticationPlugin
  {
    /// <summary>
    /// The byte array representation of the public key provided by the server.
    /// </summary>
    protected byte[] rawPubkey;

    public override string PluginName => "sha256_password";

    protected override byte[] MoreData(byte[] data)
    {
      rawPubkey = data;
      byte[] buffer = GetNonLengthEncodedPassword();
      return buffer;
    }

    public override object GetPassword()
    {
      if (Settings.SslMode != MySqlSslMode.None)
      {
        byte[] passBytes = Encoding.GetBytes(Settings.Password);
        byte[] buffer = new byte[passBytes.Length + 2];
        Array.Copy(passBytes, 0, buffer, 1, passBytes.Length);
        buffer[0] = (byte) (passBytes.Length+1);
        buffer[buffer.Length-1] = 0x00;
        return buffer;
      }
      else
      {
        if (Settings.Password.Length == 0) return new byte[1];
        // Send RSA encrypted, since the channel is not protected.
        else if (rawPubkey == null) return new byte[] { 0x01 };
        else if (!Settings.AllowPublicKeyRetrieval)
          throw new MySqlException(Resources.RSAPublicKeyRetrievalNotEnabled);
        else
        {
          byte[] bytes = GetRsaPassword(Settings.Password, AuthenticationData, rawPubkey);
          if (bytes != null && bytes.Length == 1 && bytes[0] == 0) return null;
          return bytes;
        }
      }
    }

    private byte[] GetNonLengthEncodedPassword()
    {
      // Required for AuthChange requests.
      if (Settings.SslMode != MySqlSslMode.None)
      {
        // Send as clear text, since the channel is already encrypted.
        byte[] passBytes = Encoding.GetBytes(Settings.Password);
        byte[] buffer = new byte[passBytes.Length + 1];
        Array.Copy(passBytes, 0, buffer, 0, passBytes.Length);
        buffer[passBytes.Length] = 0;
        return buffer;
      }
      else return GetPassword() as byte[];
    }

    private byte[] GetRsaPassword(string password, byte[] seedBytes, byte[] rawPublicKey)
    {
      if (password.Length == 0) return new byte[1];

      // Obfuscate the plain text password with the session scramble.
      byte[] obfuscated = GetXor(AliasText.Encoding.Default.GetBytes(password), seedBytes);

      // Encrypt the password and send it to the server.
#if NETSTANDARD1_3
      RSA rsa = MySqlPemReader.ConvertPemToRSAProvider(rawPublicKey);
      if (rsa == null)
        throw new MySqlException(Resources.UnableToReadRSAKey);
      return rsa.Encrypt(obfuscated, RSAEncryptionPadding.OaepSHA1);
#else
      RSACryptoServiceProvider rsa = MySqlPemReader.ConvertPemToRSAProvider(rawPublicKey);
      if (rsa == null)
        throw new MySqlException(Resources.UnableToReadRSAKey);

      return rsa.Encrypt(obfuscated, true);
#endif
        }

    /// <summary>
    /// Applies XOR to the byte arrays provided as input.
    /// </summary>
    /// <returns>A byte array that contains the results of the XOR operation.</returns>
    protected byte[] GetXor( byte[] src, byte[] pattern )
    {
      byte[] src2 = new byte[src.Length + 1];
      Array.Copy(src, 0, src2, 0, src.Length);
      src2[src.Length] = 0;
      byte[] result = new byte[src2.Length];
      for (int i = 0; i < src2.Length; i++)
      {
        result[ i ] = ( byte )( src2[ i ] ^ ( pattern[ i % pattern.Length ] ));
      }
      return result;
    }
  }
}
