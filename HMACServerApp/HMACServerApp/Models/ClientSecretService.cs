using Microsoft.EntityFrameworkCore;

namespace HMACServerApp.Models
{
    public class ClientSecretService
    {
        private readonly HMACDbContext _context;
        public ClientSecretService(HMACDbContext context)
        {
            _context = context;
        }
        public async Task<string?> GetSecretKeyAsync(string clientId)
        {
            var client = await _context.ClientSecrets
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.ClientId == clientId);
            return client?.SecretKey;
        }
    }
}
