using ArchiveService.Management.App.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace ArchiveService.Management.App.Data
{
    public class SettingsService : ServiceBase
    {
        private readonly IConfiguration _config;
        public SettingsService(ArchiveServiceContext context, IConfiguration config) : base(context) 
        {
            _config = config;
        }

        public async Task<ArchiveServiceSetting> GetServiceSettingsAsync()
        {
            try
            {
                var settings = await _context.ArchiveServiceSettings.FirstOrDefaultAsync();
                var url = _config.GetValue<string>("dataverseurl");
                if (settings == null)
                {
                    return new ArchiveServiceSetting() { DataverseUrl = url};
                }
                else
                {
                    settings.DataverseUrl = url;
                    return settings;
                }
            }
            catch (Exception e)
            {

                throw;
            }
        }

        public async Task<ArchiveServiceSetting> SaveServiceSettingsAsync(ArchiveServiceSetting setting)
        {
            var existing = _context.Set<ArchiveServiceSetting>().Local.FirstOrDefault(entry => entry.Id.Equals(setting.Id));
            // check if local is not null
            if (existing != null)
            {
                // detach
                _context.Entry(existing).State = EntityState.Detached;
            }
            _context.Entry(setting).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return setting;
        }
    }
}
