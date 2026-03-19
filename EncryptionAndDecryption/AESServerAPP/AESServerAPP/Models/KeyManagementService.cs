using Microsoft.EntityFrameworkCore;

namespace AESServerAPP.Models
{
    // Service for managing AES keys and IVs for clients.
    public class KeyManagementService
    {
        private readonly ApplicationDbContext _context;
        public KeyManagementService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<ClientKeyIV?> GetKeyAndIVAsync(string clientId)
        {
            // Retrieve the ClientKeyIV record matching the provided clientId, case-insensitive.
            return await _context.ClientKeyIVs
                .FirstOrDefaultAsync(c => c.ClientId.ToLower() == clientId.ToLower());
        }
    }
}
