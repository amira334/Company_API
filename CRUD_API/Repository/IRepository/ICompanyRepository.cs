using Company_API.Models;

namespace Company_API.Repository.IRepository
{
    public interface ICompanyRepository: IRepository<Company>
    {
        Task<Company> UpdateAsync(Company entity);
    }
}
