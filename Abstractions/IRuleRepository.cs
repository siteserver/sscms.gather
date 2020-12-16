using SSCMS.Gather.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SSCMS.Gather.Abstractions
{
    public interface IRuleRepository
    {
        Task InsertAsync(Rule rule);

        Task UpdateAsync(Rule rule);

        Task DeleteAsync(int ruleId);

        Task<Rule> GetAsync(int ruleId);

        Task<Rule> GetByRuleNameAsync(int siteId, string ruleName);

        Task<IEnumerable<(int Id, int SiteId)>> GetRuleIdsAsync(List<int> includes, List<int> excludes);

        Task<bool> IsExistsAsync(int siteId, string ruleName);

        Task<string> GetImportRuleNameAsync(int siteId, string ruleName);

        Task<List<Rule>> GetRulesAsync(int siteId);

        Task<List<string>> GetRuleNamesAsync(int siteId);

        Task UpdateLastGatherDateAsync(int ruleId);
    }
}
