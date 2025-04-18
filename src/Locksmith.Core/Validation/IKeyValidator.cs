using Locksmith.Core.Models;

namespace Locksmith.Core.Validation;

public interface IKeyValidator<in T> where T : KeyDescriptor
{
    void Validate(T descriptor);
}
