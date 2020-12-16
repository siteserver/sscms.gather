using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Datory;
using SSCMS.Gather.Abstractions;
using SSCMS.Gather.Models;
using SSCMS.Services;

namespace SSCMS.Gather.Core
{
    public class RuleRepository : IRuleRepository
    {
        private readonly Repository<Rule> _repository;

        public RuleRepository(ISettingsManager settingsManager)
        {
            _repository = new Repository<Rule>(settingsManager.Database, settingsManager.Redis);
        }

        private static class Attr
        {
            public const string Id = nameof(Rule.Id);
            public const string RuleName = nameof(Rule.RuleName);
            public const string SiteId = nameof(Rule.SiteId);
            public const string LastGatherDate = nameof(Rule.LastGatherDate);
        }

        public async Task InsertAsync(Rule rule)
        {
            await _repository.InsertAsync(rule);
        }

        public async Task UpdateLastGatherDateAsync(int ruleId)
        {
            await _repository.UpdateAsync(Q
                .Set(Attr.LastGatherDate, DateTime.Now)
                .Where(Attr.Id, ruleId)
            );
        }

        public async Task UpdateAsync(Rule rule)
        {
            await _repository.UpdateAsync(rule);
        }

        public async Task DeleteAsync(int ruleId)
        {
            await _repository.DeleteAsync(Q
                .Where(Attr.Id, ruleId)
            );
        }

        public async Task<Rule> GetAsync(int ruleId)
        {
            return await _repository.GetAsync(ruleId);
        }

        public async Task<Rule> GetByRuleNameAsync(int siteId, string ruleName)
        {
            return await _repository.GetAsync(Q
                .Where(Attr.SiteId, siteId)
                .Where(Attr.RuleName, ruleName)
            );
        }

        public async Task<IEnumerable<(int Id, int SiteId)>> GetRuleIdsAsync(List<int> includes, List<int> excludes)
        {
            var query = Q
                .Select(Attr.Id, Attr.SiteId)
                .OrderByDesc(Attr.Id);

            if (includes != null && includes.Count > 0)
            {
                query.WhereIn(Attr.Id, includes);
            }
            else if (excludes != null && excludes.Count > 0)
            {
                query.WhereNotIn(Attr.Id, excludes);
            }

            return await _repository.GetAllAsync<(int Id, int SiteId)>(query);
        }

        public async Task<bool> IsExistsAsync(int siteId, string ruleName)
        {
            return await _repository.ExistsAsync(Q
                .Where(Attr.RuleName, ruleName)
                .Where(Attr.SiteId, siteId)
            );
        }

        public async Task<string> GetImportRuleNameAsync(int siteId, string ruleName)
        {
            string importRuleName;
            if (ruleName != null && ruleName.IndexOf("_", StringComparison.Ordinal) != -1)
            {
                var gatherRuleNameCount = 0;
                var lastRuleName = ruleName.Substring(ruleName.LastIndexOf("_", StringComparison.Ordinal) + 1);
                var firstRuleName = ruleName.Substring(0, ruleName.Length - lastRuleName.Length);

                try
                {
                    gatherRuleNameCount = int.Parse(lastRuleName);
                }
                catch
                {
                    // ignored
                }

                gatherRuleNameCount++;
                importRuleName = firstRuleName + gatherRuleNameCount;
            }
            else
            {
                importRuleName = ruleName + "_1";
            }

            if (await _repository.ExistsAsync(Q
                .Where(Attr.RuleName, ruleName)
                .Where(Attr.SiteId, siteId)
            ))
            {
                importRuleName = await GetImportRuleNameAsync(siteId, importRuleName);
            }

            return importRuleName;
        }

        public async Task<List<Rule>> GetRulesAsync(int siteId)
        {
            return await _repository.GetAllAsync(Q
                .Where(Attr.SiteId, siteId)
                .OrderByDesc(Attr.Id)
            );
        }

        public async Task<List<string>> GetRuleNamesAsync(int siteId)
        {
            return await _repository.GetAllAsync<string>(Q
                .Select(Attr.RuleName)
                .Where(Attr.SiteId, siteId)
            );
        }
    }
}
