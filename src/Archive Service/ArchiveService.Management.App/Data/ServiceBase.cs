using ArchiveService.Management.App.Data.Models;

namespace ArchiveService.Management.App.Data
{
    public abstract class ServiceBase
    {
        protected ArchiveServiceContext _context;
        public ServiceBase(ArchiveServiceContext context)
        {
            _context = context;
        }
    }
}
