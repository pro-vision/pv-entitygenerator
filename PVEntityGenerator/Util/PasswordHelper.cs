using System;
using System.Text;
using System.Security.Cryptography;

namespace PVEntityGenerator.Util {

  public class PasswordHelper {

    private static byte[] DES_KEY = Encoding.ASCII.GetBytes("The Key.");
    private static byte[] DES_IV = Encoding.ASCII.GetBytes("The IV.");

    public static string EncryptPassword(string pPassword) {
      byte[] abytPassword = Encoding.Unicode.GetBytes(pPassword);

      ICryptoTransform crypt = GetCryptoTransform(true);
      byte[] abytCrypt = crypt.TransformFinalBlock(abytPassword, 0, abytPassword.Length);

      return System.Convert.ToBase64String(abytCrypt);
    }

    public static string DecryptPassword(string pPassword) {
      byte[] abytCrypt = System.Convert.FromBase64String(pPassword);

      ICryptoTransform crypt = GetCryptoTransform(false);
      byte[] abytPassword = crypt.TransformFinalBlock(abytCrypt, 0, abytCrypt.Length);

      return Encoding.Unicode.GetString(abytPassword);
    }

    private static ICryptoTransform GetCryptoTransform(bool pfEncrypt) {
      DESCryptoServiceProvider des = new DESCryptoServiceProvider();
      des.Key = DES_KEY;
      des.IV = DES_IV;

      if (pfEncrypt) {
        return des.CreateEncryptor();
      }
      else {
        return des.CreateDecryptor();
      }
    }

  }

}
