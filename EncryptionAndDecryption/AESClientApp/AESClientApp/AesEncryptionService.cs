using System.Security.Cryptography;
using System.Text;

namespace AESClientApp
{
    // Service for performing AES encryption and decryption operations.
    public class AesEncryptionService
    {
        private readonly byte[] _key; // Holds the AES encryption key.
        private readonly byte[] _iv; // Holds the AES initialization vector (IV).

        // Constructor to initialize the encryption service with a key and IV.
        public AesEncryptionService(string base64Key, string base64IV)
        {
            // Validate that the provided key is not null or empty.
            if (string.IsNullOrWhiteSpace(base64Key))
                throw new ArgumentException("Key cannot be null or empty.", nameof(base64Key));

            // Validate that the provided IV is not null or empty.
            if (string.IsNullOrWhiteSpace(base64IV))
                throw new ArgumentException("IV cannot be null or empty.", nameof(base64IV));

            // Convert the Base64-encoded key string into a byte array for AES.
            _key = Convert.FromBase64String(base64Key);

            // Convert the Base64-encoded IV string into a byte array for AES.
            _iv = Convert.FromBase64String(base64IV);
        }

        // Encrypts a plaintext string using AES encryption.
        public string EncryptString(string plainText)
        {
            // Validate that the plaintext is not null.
            if (plainText == null)
                throw new ArgumentNullException(nameof(plainText));

            // Create an instance of the AES algorithm.
            using (Aes aesAlg = Aes.Create())
            {
                // Assign the pre-initialized key to the AES instance.
                aesAlg.Key = _key;

                // Assign the pre-initialized IV to the AES instance.
                aesAlg.IV = _iv;

                // Create an encryptor object for transforming plaintext into ciphertext.
                ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);

                // Convert the plaintext string into a byte array using UTF-8 encoding.
                byte[] plainBytes = Encoding.UTF8.GetBytes(plainText);

                // Perform the encryption operation on the plaintext bytes.
                byte[] encryptedBytes = encryptor.TransformFinalBlock(plainBytes, 0, plainBytes.Length);

                // Convert the encrypted byte array into a Base64-encoded string and return it.
                return Convert.ToBase64String(encryptedBytes);
            }
        }

        // Decrypts a ciphertext string back to plaintext using AES decryption.
        public string DecryptString(string cipherText)
        {
            // Validate that the ciphertext is not null.
            if (cipherText == null)
                throw new ArgumentNullException(nameof(cipherText));

            // Create an instance of the AES algorithm.
            using (Aes aesAlg = Aes.Create())
            {
                // Assign the pre-initialized key to the AES instance.
                aesAlg.Key = _key;

                // Assign the pre-initialized IV to the AES instance.
                aesAlg.IV = _iv;

                // Create a decryptor object for transforming ciphertext back into plaintext.
                ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);

                // Convert the Base64-encoded ciphertext string into a byte array.
                byte[] cipherBytes = Convert.FromBase64String(cipherText);

                // Perform the decryption operation on the ciphertext bytes.
                byte[] decryptedBytes = decryptor.TransformFinalBlock(cipherBytes, 0, cipherBytes.Length);

                // Convert the decrypted byte array into a plaintext string using UTF-8 encoding and return it.
                return Encoding.UTF8.GetString(decryptedBytes);
            }
        }
    }
}