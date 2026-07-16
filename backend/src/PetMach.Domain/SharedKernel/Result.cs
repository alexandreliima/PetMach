namespace PetMach.Domain.SharedKernel;

public class Result
{
    protected Result(bool isSuccess, DomainError error)
    {
        if (isSuccess && error != DomainError.None || !isSuccess && error == DomainError.None)
        {
            throw new ArgumentException("O estado do resultado e o erro são inconsistentes.", nameof(error));
        }

        IsSuccess = isSuccess;
        Error = error;
    }

    public bool IsSuccess { get; }

    public bool IsFailure => !IsSuccess;

    public DomainError Error { get; }

    public static Result Success() => new(true, DomainError.None);

    public static Result Failure(DomainError error) => new(false, error);

    public static Result<TValue> Success<TValue>(TValue value) => new(value);

    public static Result<TValue> Failure<TValue>(DomainError error) => new(error);
}

public sealed class Result<TValue> : Result
{
    private readonly TValue? value;

    internal Result(TValue value)
        : base(true, DomainError.None) => this.value = value;

    internal Result(DomainError error)
        : base(false, error) => value = default;

    public TValue Value => IsSuccess
        ? value!
        : throw new InvalidOperationException("Um resultado com falha não possui valor.");
}
