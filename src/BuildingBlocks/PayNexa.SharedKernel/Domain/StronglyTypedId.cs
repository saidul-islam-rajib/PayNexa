namespace PayNexa.SharedKernel.Domain;

public abstract class StronglyTypedId(Guid value) : SingleValueObject<Guid>(value);
