namespace NDE.Application.Interfaces;

public interface ITeamStandardService
{
  Task<bool> SaveTeamStandardAsync(object teamStandard, Guid repositoryId);
  Task<object?> GetTeamStandardAsync(Guid repositoryId);
}
