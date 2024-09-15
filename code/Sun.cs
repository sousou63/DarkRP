using System.Numerics;
using Sandbox;

public sealed class Sun : Component,Component.ExecuteInEditor
{
	[Property] public SkyBox2D SkyBox {get; set;}
	[Property] public Material SkyBoxMaterial {get; set;}
	// Charger les matériaux de jour et de nuit

    [Property] public float SpeedTime {get; set;}

    [Property] public SoundPointComponent DaySoundAmbient {get; set;}

    [Property] public SoundPointComponent NightSoundAmbient {get; set;}
    
    
    Material night = Material.Load("materials/skybox/skybox_dark_01.vmat");
    Material day = Material.Load("materials/skybox/skybox_day_01.vmat");
	public void UpdateSunDirection()
	{
    	// Définir la vitesse de rotation (ajustable selon les besoins)
    float rotationSpeed = SpeedTime; // Par exemple, 5° par seconde
	// Calcul du changement d'angle basé sur le temps écoulé
        float deltaTime = Time.Delta;
        float pitchChange = rotationSpeed * deltaTime;

        // Mise à jour de la rotation actuelle du GameObject
        Rotation currentRotation = GameObject.Transform.Rotation;
        Rotation pitchRotation = Rotation.FromAxis(Vector3.Right, pitchChange);
        Rotation newRotation = pitchRotation * currentRotation;

        // Appliquer la nouvelle rotation au GameObject
        GameObject.Transform.Rotation = newRotation;

        // Récupérer le pitch actuel
        float currentPitch = newRotation.Pitch();
        
        // Normaliser le pitch pour s'assurer qu'il est dans la plage [0, 360]
        currentPitch = currentPitch.NormalizeDegrees();
        
        if (currentPitch < 0) currentPitch += 360;

        // Définir les seuils pour la nuit et le jour
        const float nightStart = 180f;  // Commence à 180° (coucher de soleil)
        const float dayStart = 360f;    // Commence à 360° (lever de soleil)

        // Loguer le pitch pour le débogage
        Log.Info($"Current Pitch: {currentPitch}");

        // Déterminer si c'est la nuit ou le jour
        if (currentPitch >= nightStart && currentPitch < 360f)
        {
            // C'est la nuit
            Log.Info("Night");
           /* SkyBox.SkyMaterial = test;*/
            SkyBox.SkyMaterial = SkyBoxMaterial;
            SkyBox.Tint = Color.Gray;
            DaySoundAmbient.StopSound();
            NightSoundAmbient.StartSound();
        }
        else if (currentPitch < dayStart || currentPitch >= 0f)
        {
            // C'est le jour
            Log.Info("Day");
            SkyBox.SkyMaterial = day;
            SkyBox.Tint = Color.White;
            DaySoundAmbient.StartSound();
            NightSoundAmbient.StopSound();
        }
}
	


	protected override void OnFixedUpdate()
	{
		UpdateSunDirection();
	}

	protected override void OnUpdate()
	{
		
	}
}
