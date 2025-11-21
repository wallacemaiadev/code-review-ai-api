namespace NDE.Domain.Entities.Common
{
  public class JobStatus : Type<int>
  {
    public static readonly JobStatus Pending = new JobStatus(1, "Pending");
    public static readonly JobStatus Running = new JobStatus(2, "Running");
    public static readonly JobStatus Finished = new JobStatus(3, "Finished");
    public static readonly JobStatus Error = new JobStatus(-1, "Error");
    public static readonly JobStatus Dead = new JobStatus(-2, "Dead");
    public static readonly JobStatus Canceled = new JobStatus(-3, "Canceled");

    private JobStatus(int id, string description)
    {
      Id = id;
      Description = description;
    }

    public static JobStatus FromId(int id)
    {
      return id switch
      {
        1 => Pending,
        2 => Running,
        3 => Finished,
        -1 => Error,
        -2 => Dead,
        -3 => Canceled,
        _ => throw new ArgumentOutOfRangeException(nameof(id), id, $"Não foi possível encontrar o status do Job com o ID: {id}")
      };
    }

    public static implicit operator JobStatus(int id) => FromId(id);
  }
}
