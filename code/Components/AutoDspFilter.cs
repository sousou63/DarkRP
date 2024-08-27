using System;
using Sandbox.Audio;

namespace Scenebox;

public sealed class AutoDspFilter : Component
{
    [Property] List<DspPresetHandle> OutsidePresets { get; set; }
    [Property, Range( 0f, 1f )] float OutsideMix { get; set; } = 1f;

    [Property] List<DspPresetHandle> ConcretePresets { get; set; }
    [Property, Range( 0f, 1f )] float ConcreteMix { get; set; } = 1f;

    [Property] List<DspPresetHandle> PlasterPresets { get; set; }
    [Property, Range( 0f, 1f )] float PlasterMix { get; set; } = 1f;

    [Property] List<DspPresetHandle> MetalPresets { get; set; }
    [Property, Range( 0f, 1f )] float MetalMix { get; set; } = 1f;

    [Property] List<DspPresetHandle> WoodPresets { get; set; }
    [Property, Range( 0f, 1f )] float WoodMix { get; set; } = 1f;

    Mixer mixer;
    DspProcessor processor;
    List<DspProcessor> previousProcessors = new();

    float CurrentMix = 0f;
    DspPresetHandle CurrentPreset
    {
        get => _currentPreset;
        set
        {
            if ( _currentPreset == value ) return;

            previousProcessors.Add( processor );
            processor = new DspProcessor();
            mixer.AddProcessor( processor );
            processor.Mix = 0;
            processor.Effect = value;
            _currentPreset = value;
        }
    }
    DspPresetHandle _currentPreset;
    TimeSince timeSinceLastUpdate = 10f;
    float Distance => 6000f;

    protected override void OnStart()
    {
        mixer = Mixer.FindMixerByName( "Game" );
        processor = new DspProcessor();
        mixer.AddProcessor( processor );
        processor.Mix = 0;
    }

    protected override void OnUpdate()
    {
        for ( int i = previousProcessors.Count - 1; i >= 0; i-- )
        {
            var prev = previousProcessors[i];
            prev.Mix = prev.Mix.LerpTo( 0, 10 * Time.Delta );
            if ( prev.Mix <= 0.01f )
            {
                mixer.RemoveProcessor( prev );
                previousProcessors.Remove( prev );
            }
        }


        processor.Mix = processor.Mix.LerpTo( CurrentMix, 10 * Time.Delta );
    }

    protected override void OnFixedUpdate()
    {
        if ( timeSinceLastUpdate < 0.2f ) return;

        (DspPresetHandle preset, float mix) = GetPresetAndMix();
        CurrentPreset = preset;
        CurrentMix = mix;

        timeSinceLastUpdate = 0;
    }

    (DspPresetHandle, float) GetPresetAndMix()
    {
        var upwardTr = Scene.Trace.Ray( new Ray( Transform.Position, Vector3.Up ), Distance )
            .WithoutTags( "player", "trigger" )
            .Run();

        float totalSize = 0;
        List<Surface> surfaces = new();

        totalSize += upwardTr.Distance;
        surfaces.Add( upwardTr.Surface );

        var downwardTr = Scene.Trace.Ray( new Ray( Transform.Position, Vector3.Down ), Distance )
            .WithoutTags( "player", "trigger" )
            .Run();

        totalSize += downwardTr.Distance;
        surfaces.Add( downwardTr.Surface );

        for ( int i = 0; i < 8; i++ )
        {
            var tr = Scene.Trace.Ray( new Ray( Transform.Position, new Vector3(
                MathF.Sin( i * MathF.PI / 4 ),
                MathF.Cos( i * MathF.PI / 4 ),
                0
            ) ), Distance )
                .WithoutTags( "player", "trigger" )
                .Run();

            totalSize += tr.Distance;
            surfaces.Add( tr.Surface );
        }

        int size = 2;
        if ( totalSize < 2_000 ) size = 0;
        else if ( totalSize < 20_000 ) size = 1;

        var commonSurface = surfaces.GroupBy( x => x ).OrderByDescending( x => x.Count() ).First().Key;

        switch ( commonSurface.ResourceName )
        {
            case "plaster":
            case "plastic":
                return (PlasterPresets[size], PlasterMix);
            case "brick":
            case "concrete":
            case "ceramic":
                return (ConcretePresets[size], ConcreteMix);
            case "metal":
            case "metal.sheet":
                return (MetalPresets[size], MetalMix);
            case "wood":
            case "wood.sheet":
                return (WoodPresets[size], WoodMix);
            case "grass":
            case "dirt":
            case "sand":
            case "gravel":
            case "mud":
                return (OutsidePresets[size], OutsideMix);
            default:
                if ( size < 2 )
                {
                    return (PlasterPresets[size], PlasterMix);
                }
                else
                {
                    return (OutsidePresets[size], OutsideMix);
                }
        }
    }
}