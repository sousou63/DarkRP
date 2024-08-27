namespace Scenebox;

public interface ICameraOverride
{
    bool IsActive { get; }

    void UpdateCamera();
}