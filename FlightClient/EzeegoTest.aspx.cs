using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

using System.Security.Cryptography;

namespace FlightClient
{
    public partial class EzeegoTest : System.Web.UI.Page
    {
        public int iterationCount = 1000;
        public int keySize = 128;
        public string passPhrase = "never give up on encryption logic";

        protected void Page_Load(object sender, EventArgs e)
        {

        }

        protected void btnEnc_Click(object sender, EventArgs e)
        {

        }

        /*
        public static byte[] CreateKey(string password, byte[] salt)
        {
            
            using (var rfc2898DeriveBytes = new Rfc2898DeriveBytes(password, salt, iterations))
                return rfc2898DeriveBytes.GetBytes(32);
            
        }
        */

        private static byte[] GetSalt()
        {
            var salt = new byte[32];
            using (var random = new RNGCryptoServiceProvider())
            {
                random.GetNonZeroBytes(salt);
            }

            return salt;
        }

        /*
         * 
         private static readonly int iterations = 1000;

    public static string Encrypt(string input, string password)
    {
        byte[] encrypted;
        byte[] IV;
        byte[] Salt = GetSalt();
        byte[] Key = CreateKey(password, Salt);

        using (Aes aesAlg = Aes.Create())
        {
            aesAlg.Key = Key;
            aesAlg.Padding = PaddingMode.PKCS7;
            aesAlg.Mode = CipherMode.CBC;

            aesAlg.GenerateIV();
            IV = aesAlg.IV;

            var encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);

            using (var msEncrypt = new MemoryStream())
            {
                using (var csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                {
                    using (var swEncrypt = new StreamWriter(csEncrypt))
                    {
                        swEncrypt.Write(input);
                    }
                    encrypted = msEncrypt.ToArray();
                }
            }
        }

        byte[] combinedIvSaltCt = new byte[Salt.Length + IV.Length + encrypted.Length];
        Array.Copy(Salt, 0, combinedIvSaltCt, 0, Salt.Length);
        Array.Copy(IV, 0, combinedIvSaltCt, Salt.Length, IV.Length);
        Array.Copy(encrypted, 0, combinedIvSaltCt, Salt.Length + IV.Length, encrypted.Length);

        return Convert.ToBase64String(combinedIvSaltCt.ToArray());
    }

    public static string Decrypt(string input, string password)
    {
        byte[] inputAsByteArray;
        string plaintext = null;
        try
        {
            inputAsByteArray = Convert.FromBase64String(input);

            byte[] Salt = new byte[32];
            byte[] IV = new byte[16];
            byte[] Encoded = new byte[inputAsByteArray.Length - Salt.Length - IV.Length];

            Array.Copy(inputAsByteArray, 0, Salt, 0, Salt.Length);
            Array.Copy(inputAsByteArray, Salt.Length, IV, 0, IV.Length);
            Array.Copy(inputAsByteArray, Salt.Length + IV.Length, Encoded, 0, Encoded.Length);

            byte[] Key = CreateKey(password, Salt);

            using (Aes aesAlg = Aes.Create())
            {
                aesAlg.Key = Key;
                aesAlg.IV = IV;
                aesAlg.Mode = CipherMode.CBC;
                aesAlg.Padding = PaddingMode.PKCS7;

                ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);

                using (var msDecrypt = new MemoryStream(Encoded))
                {
                    using (var csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                    {
                        using (var srDecrypt = new StreamReader(csDecrypt))
                        {
                            plaintext = srDecrypt.ReadToEnd();
                        }
                    }
                }
            }

            return plaintext;
        }
        catch (Exception e)
        {
            return null;
        }
    }

    public static byte[] CreateKey(string password, byte[] salt)
    {
        using (var rfc2898DeriveBytes = new Rfc2898DeriveBytes(password, salt, iterations))
            return rfc2898DeriveBytes.GetBytes(32);
    }

    private static byte[] GetSalt()
    {
        var salt = new byte[32];
        using (var random = new RNGCryptoServiceProvider())
        {
            random.GetNonZeroBytes(salt);
        }

        return salt;
    }
For the Javascript solution I am using CryptoJS, based upon this reference http://www.adonespitogo.com/articles/encrypting-data-with-cryptojs-aes/

    var keySize = 256;
var ivSize = 128;
var saltSize = 256;
var iterations = 1000;

var message = "Hello World";
var password = "Secret Password";


function encrypt (msg, pass) {
  var salt = CryptoJS.lib.WordArray.random(saltSize/8);

  var key = CryptoJS.PBKDF2(pass, salt, {
      keySize: keySize/32,
      iterations: iterations
    });

  var iv = CryptoJS.lib.WordArray.random(ivSize/8);

  var encrypted = CryptoJS.AES.encrypt(msg, key, { 
    iv: iv, 
    padding: CryptoJS.pad.Pkcs7,
    mode: CryptoJS.mode.CBC

  });

  // salt, iv will be hex 32 in length
  // append them to the ciphertext for use  in decryption
  var transitmessage = salt + iv + encrypted;
  return transitmessage.toString();
}

function decrypt (transitmessage, pass) {
  var salt = CryptoJS.enc.Hex.parse(transitmessage.substr(0, 64));
  var iv = CryptoJS.enc.Hex.parse(transitmessage.substr(64, 32));
  var encrypted = transitmessage.substring(96);

  var key = CryptoJS.PBKDF2(pass, salt, {
      keySize: keySize/32,
      iterations: iterations
    });

  var decrypted = CryptoJS.AES.decrypt(encrypted, key, { 
    iv: iv, 
    padding: CryptoJS.pad.Pkcs7,
    mode: CryptoJS.mode.CBC

  })
  return decrypted.toString(CryptoJS.enc.Utf8);
}
         */
    }
}