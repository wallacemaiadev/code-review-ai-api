using NDE.Domain.Entities.Common;

namespace NDE.Domain.Entities.CodeReviews;

public class Verdict : Type<int>
{
  public static Verdict Reproved = new Verdict { Id = 0, Description = "Reprovado" };
  public static Verdict Pending = new Verdict { Id = 1, Description = "Pendente" };
  public static Verdict Partial = new Verdict { Id = 2, Description = "Parcial" };
  public static Verdict Approved = new Verdict { Id = 3, Description = "Aprovado" };
  public static Verdict Abandoned = new Verdict { Id = -1, Description = "Abandonado" };
  public static Verdict NoFeedback = new Verdict { Id = -2, Description = "Sem Feedback" };

  public static Verdict FromId(int id)
  {
    return id switch
    {
      0 => Reproved,
      1 => Pending,
      2 => Partial,
      3 => Approved,
      -1 => Abandoned,
      -2 => NoFeedback,
      _ => throw new ArgumentNullException($"Não foi possível encontrar o veredito com o ID: {id}")
    };
  }

  public static implicit operator Verdict(int id) => FromId(id);
}
