using System.Security.Cryptography;

namespace KeyGeneratorService
{
    // Main class of the KeyGeneratorService program
    public class Program
    {
        // Entry point of the console application
        static void Main(string[] args)
        {
            // Call the method to generate AES key and IV
            GenerateAesKeyAndIV();

            // Wait for a key press to prevent the console from closing immediately
            Console.ReadKey();
        }

        // Method to generate an AES key and Initialization Vector (IV)
        private static void GenerateAesKeyAndIV()
        {
            // Create a new AES object to generate keys.
            using (Aes aesAlg = Aes.Create())
            {
                // Set the size of the encryption key to 256 bits, offering strong security
                aesAlg.KeySize = 256;

                // Generate a random key based on the key size set above
                aesAlg.GenerateKey();

                // Generate a random initialization vector (IV)
                aesAlg.GenerateIV();

                // Convert the generated key to a base64 string for easier readability and storage
                string key = Convert.ToBase64String(aesAlg.Key);

                // Convert the generated IV to a base64 string for easier readability and storage
                string iv = Convert.ToBase64String(aesAlg.IV);

                // Output the base64-encoded AES key to the console
                Console.WriteLine("AES Key (Base64): " + key);

                // Output the base64-encoded AES IV to the console
                Console.WriteLine("AES IV (Base64): " + iv);
            }
        }
    }
}