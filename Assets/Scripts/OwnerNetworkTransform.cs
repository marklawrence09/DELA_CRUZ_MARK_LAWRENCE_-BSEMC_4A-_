using Unity.Netcode.Components;

public class OwnerNetworkTransform : NetworkTransform
{
    protected override bool OnIsServerAuthoritative()
    {
        return false; // Allows client owners to sync position to host and other clients
    }
}