using System.Security.Cryptography;
using System.Text;

namespace AESServerAPP.Models
{
    // Service class for performing AES encryption and decryption operations.
    public class AesEncryptionService
    {
        // Dependency to retrieve the AES key and IV for a specific client.
        private readonly KeyManagementService _keyManagementService;

        // Constructor to inject KeyManagementService as a dependency.
        public AesEncryptionService(KeyManagementService keyManagementService)
        {
            // Assign the injected dependency to the private field.
            _keyManagementService = keyManagementService;
        }

        // Method to convert plain text into encrypted text.
        public async Task<string> EncryptStringAsync(string clientId, string plainText)
        {
            // Retrieve the AES key and IV for the specified client from the key management service.
            var client = await _keyManagementService.GetKeyAndIVAsync(clientId);

            // If no matching client configuration is found, throw an exception.
            if (client == null)
                throw new ArgumentException("Invalid Client Id");

            // Decode the base64-encoded key and IV into byte arrays for AES initialization.
            byte[] key = Convert.FromBase64String(client.Key); // Convert the Base64 string to a byte array (AES key).
            byte[] iv = Convert.FromBase64String(client.IV); // Convert the Base64 string to a byte array (Initialization Vector).

            // Create and configure an AES encryption algorithm instance.
            using (Aes aesAlg = Aes.Create())
            {
                // Assign the retrieved key and IV to the AES instance.
                aesAlg.Key = key; // Assign the decoded key to the AES instance.
                aesAlg.IV = iv; // Assign the decoded IV to the AES instance.

                // Create an encryptor object to perform the encryption transformation.
                ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);

                // Convert the plaintext string into a byte array using UTF-8 encoding.
                byte[] plainBytes = Encoding.UTF8.GetBytes(plainText);

                // Perform encryption on the plaintext bytes and get the encrypted data.
                byte[] encryptedBytes = encryptor.TransformFinalBlock(plainBytes, 0, plainBytes.Length);

                // Convert the encrypted byte array to a Base64-encoded string and return it.
                return Convert.ToBase64String(encryptedBytes);
            }
        }

        // Method to convert encrypted text back into plain text.
        public async Task<string> DecryptStringAsync(string clientId, string cipherText)
        {
            // Retrieve the AES key and IV for the specified client from the key management service.
            var client = await _keyManagementService.GetKeyAndIVAsync(clientId);

            // If no matching client configuration is found, throw an exception.
            if (client == null)
                throw new ArgumentException("Invalid Client Id");

            // Decode the base64-encoded key and IV into byte arrays for AES initialization.
            byte[] key = Convert.FromBase64String(client.Key); // Convert the Base64 string to a byte array (AES key).
            byte[] iv = Convert.FromBase64String(client.IV); // Convert the Base64 string to a byte array (Initialization Vector).

            // Decode the Base64-encoded ciphertext into a byte array for decryption.
            byte[] cipherBytes = Convert.FromBase64String(cipherText);

            // Create and configure an AES decryption algorithm instance.
            using (Aes aesAlg = Aes.Create())
            {
                aesAlg.Key = key; // Assign the decoded key to the AES instance.
                aesAlg.IV = iv; // Assign the decoded IV to the AES instance.

                // Create a decryptor object using the assigned key and IV to perform the decryption transformation.
                ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);

                // Perform decryption on the ciphertext bytes and get the original plaintext bytes.
                byte[] decryptedBytes = decryptor.TransformFinalBlock(cipherBytes, 0, cipherBytes.Length);

                // Convert the decrypted byte array into a plaintext string using UTF-8 encoding and return it.
                return Encoding.UTF8.GetString(decryptedBytes);
            }
        }
    }
}
