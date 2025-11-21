using System.ComponentModel.DataAnnotations;

namespace NDE.Application.ViewModels.Request.CodeReviews
{
  public class AzurePullRequestRequestViewModel
  {
    [Required(ErrorMessage = "A CollectionUrl é obrigatória.")]
    [MaxLength(500, ErrorMessage = "A CollectionUrl deve ter no máximo 500 caracteres.")]
    public string CollectionUrl { get; set; } = string.Empty;

    [Required(ErrorMessage = "O AuthToken é obrigatório.")]
    [MaxLength(500, ErrorMessage = "O token deve ter no máximo 500 caracteres.")]
    public string AuthToken { get; set; } = string.Empty;

    [Required(ErrorMessage = "O PullRequestId é obrigatório.")]
    public int PullRequestId { get; set; }

    [Required(ErrorMessage = "O ProjectName é obrigatório.")]
    [MaxLength(200, ErrorMessage = "O ProjectName deve ter no máximo 200 caracteres.")]
    public string ProjectName { get; set; } = string.Empty;

    [Required(ErrorMessage = "O RepositoryId é obrigatório.")]
    public Guid RepositoryId { get; set; }
  }
}
