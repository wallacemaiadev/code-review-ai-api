namespace NDE.VectorStore.DTOs;

public record VectorSearchResult<T>(
    double Score,
    T Payload
);
