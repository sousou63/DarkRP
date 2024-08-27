
namespace Scenebox;

public partial class ViewModel : Component
{
    public Weapon Weapon { get; set; }

    [Property] public SkinnedModelRenderer Arms { get; set; }
    [Property] public SkinnedModelRenderer ModelRenderer { get; set; }

    Player Player => Weapon.Player;

    [Property, Group( "References" )] public GameObject Muzzle { get; set; }

    private float YawInertiaScale => 2f;
    private float PitchInertiaScale => 2f;
    private bool activateInertia = false;
    private float lastPitch;
    private float lastYaw;
    private float YawInertia;
    private float PitchInertia;

    public void SetVisible( bool visible )
    {
        ModelRenderer.Enabled = visible;
        Arms.Enabled = visible;
    }

    protected override void OnStart()
    {
        ModelRenderer?.Set( "b_deploy", true );
        if ( !Network.IsOwner ) GameObject.Enabled = false;

        if ( Player.IsValid() ) Player.OnJump += OnPlayerJumped;
    }

    void OnPlayerJumped()
    {
        ModelRenderer?.Set( "b_jump", true );
    }

    void ApplyAnimationTransform()
    {
        if ( !Network.IsOwner ) return;
        if ( !ModelRenderer.IsValid() ) return;
        if ( !ModelRenderer.Enabled ) return;

        var bone = ModelRenderer.SceneModel.GetBoneLocalTransform( "camera" );
        var camera = Player.FirstPersonView;

        var scale = 1f; // TODO: View Bob Setting

        localPosition += bone.Position * scale;
        localRotation *= bone.Rotation * scale;
    }

    void ApplyInertia()
    {
        var camera = Player.FirstPersonView;
        var inRot = camera.Transform.Rotation;

        if ( !activateInertia )
        {
            lastPitch = inRot.Pitch();
            lastYaw = inRot.Yaw();
            YawInertia = 0;
            PitchInertia = 0;
            activateInertia = true;
        }

        var newPitch = camera.Transform.Rotation.Pitch();
        var newYaw = camera.Transform.Rotation.Yaw();

        PitchInertia = Angles.NormalizeAngle( newPitch - lastPitch );
        YawInertia = Angles.NormalizeAngle( newYaw - lastYaw );

        lastPitch = newPitch;
        lastYaw = newYaw;

        ModelRenderer?.Set( "aim_yaw_inertia", YawInertia * YawInertiaScale );
        ModelRenderer?.Set( "aim_pitch_inertia", PitchInertia * PitchInertiaScale );
    }

    private Vector3 lerpedWishLook;
    private Vector3 localPosition;
    private Rotation localRotation;

    private Vector3 lerpedLocalPosition;
    private Rotation lerpedLocalRotation;

    protected void ApplyVelocity()
    {
        var moveVel = Player.CharacterController.Velocity;
        var moveLen = moveVel.Length;

        var wishLook = new Vector3( 0, Input.MouseDelta.x, Input.MouseDelta.y ) * 0.01f;

        if ( Player.IsCrouching ) moveLen *= 0.2f;

        lerpedWishLook = wishLook.LerpTo( lerpedWishLook, Time.Delta * 5f );

        localRotation *= Rotation.From( 0, -lerpedWishLook.y * 3f, 0 );
        localPosition += -lerpedWishLook;

        ModelRenderer?.Set( "move_groundspeed", moveLen );
    }

    void ApplyAnimationParameters()
    {
        ModelRenderer.Set( "b_sprint", Player.IsSprinting && Player.WishVelocity.Length > 1 );
        ModelRenderer.Set( "b_grounded", Player.CharacterController.IsOnGround );

        ModelRenderer.Set( "b_twohanded", true );
    }

    protected override void OnUpdate()
    {
        localRotation = Rotation.Identity;
        localPosition = Vector3.Zero;

        if ( !Player.IsValid() || !Player.CharacterController.IsValid() ) return;

        ApplyAnimationParameters();

        ApplyVelocity();
        ApplyAnimationTransform();
        ApplyInertia();

        lerpedLocalRotation = Rotation.Lerp( lerpedLocalRotation, localRotation, Time.Delta * 10f );
        lerpedLocalPosition = lerpedLocalPosition.LerpTo( localPosition, Time.Delta * 10f );

        Transform.LocalRotation = lerpedLocalRotation;
        Transform.LocalPosition = lerpedLocalPosition;
    }

}