using NDE.Domain.Entities.Common;

namespace NDE.Domain.Entities.CodeReviews;

public class Verdict : Type<int>
{
  public static Verdict Reproved = new Verdict { Id = 0, Description = "Reprovado" };
  public static Verdict Pending = new Verdict { Id = 1, Description = "Pendente" };
  public static Verdict Approved = new Verdict { Id = 2, Description = "Aprovado" };
  public static Verdict Abondoned = new Verdict { Id = -1, Description = "Abandonado" };

  public static implicit operator Verdict(int id)
  {
    switch (id)
    {
      case 1: return Pending;
      case 2: return Approved;
      case 0: return Reproved;
      case -1: return Abondoned;
      default: throw new ArgumentNullException($"Não foi possível encontrar o status do Pull Request com o ID: {id}");
    }
  }
}
