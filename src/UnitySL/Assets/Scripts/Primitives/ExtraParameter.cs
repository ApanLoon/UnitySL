
using System;
using Assets.Scripts.Extensions.UnityExtensions;
using UnityEngine;

namespace Assets.Scripts.Primitives
{
    public enum ExtraParameterType
    {
        Flexible     = 0x10,
        Light        = 0x20,
        Sculpt       = 0x30,
        LightImage   = 0x40,
        Reserved     = 0x50, // Used on server-side
        Mesh         = 0x60,
        ExtendedMesh = 0x70,
    }

    /// <summary>
    /// Container for one of each extra parameter.
    ///
    /// </summary>
    public class ExtraParameters
    {
        public LightParameter LightParameter { get; set; }
        public FlexibleObjectData FlexibleObjectData { get; set; }
        public SculptParams SculptParams { get; set; }
        public LightImageParams LightImageParams { get; set; }
        public ExtendedMeshParams ExtendedMeshParams { get; set; }

        public override string ToString()
        {
            string s = "";
            if (LightParameter != null)
            {
                s += $"LightParameter: {{{LightParameter}}},";
            }
            if (FlexibleObjectData != null)
            {
                s += $"FlexibleObjectData: {{{FlexibleObjectData}}},";
            }
            if (SculptParams != null)
            {
                s += $"SculptParams: {{{SculptParams}}},";
            }
            if (LightImageParams != null)
            {
                s += $"LightImageParams: {{{LightImageParams}}},";
            }
            if (ExtendedMeshParams != null)
            {
                s += $"ExtendedMeshParams: {{{ExtendedMeshParams}}},";
            }
            return s;
        }
    }

    /// <summary>
    /// Base class for data of special primitive types.
    /// 
    /// NOTE: In Indra, this class is called LLNetworkData for some reason
    /// </summary>
    public class ExtraParameter
    {
        public ExtraParameterType ParameterType { get; set; }
    }

    //TODO: In indra, all the ExtraParameter classes have component-wise Equatable overrides, should we add that?

    public class LightParameter : ExtraParameter
    {
        public const float MinRadius      = 0.0f;
        public const float DefaultRadius  = 10.0f;
        public const float MaxRadius      = 20.0f;
        public const float MinFallOff     = 0.0f;
        public const float DefaultFallOff = 0.75f;
        public const float MaxFallOff     = 2.0f;
        public const float MinCutOff      = 0.0f;
        public const float DefaultCutOff  = 0.0f;
        public const float MaxCutOff      = 180f;

        /// <summary>
        /// Linear colour (not gamma corrected), alpha = intensity
        /// </summary>
        public Color Colour { get; protected set; }

        public float Radius { get => _radius; set => Mathf.Clamp(value, MinRadius, MaxRadius); }
        private float _radius;

        public float Falloff { get => _falloff; set => Mathf.Clamp(value, MinFallOff, MaxFallOff); }
        private float _falloff;

        public float Cutoff { get => _cutoff; set => Mathf.Clamp(value, MinCutOff, MaxCutOff); }
        private float _cutoff;

        public LightParameter()
        {
            Colour        = Color.white;
            _radius       = DefaultRadius;
            _falloff      = DefaultFallOff;
            _cutoff       = DefaultCutOff;
            ParameterType = ExtraParameterType.Light;
        }

        /// <summary>
        /// Set the colour by gamma corrected colour value
        /// </summary>
        /// <param name="colour">Gamma corrected colour value (directly taken from an on-screen colour swatch)</param>
        public void SetSrgbColour(Color colour)
        {
            SetLinearColour(colour.linear);
        }

        /// <summary>
        /// Set the colour by linear colour value
        /// </summary>
        /// <param name="colour">Linear colour value (value as it appears in shaders)</param>
        public void SetLinearColour(Color colour)
        {
            Colour = colour;
            Colour.Clamp();
        }

        public override string ToString()
        {
            return $"Colour: {Colour}, Radius: {Radius}, Cutoff: {Cutoff}, Falloff: {Falloff}";
        }
    }

    public class FlexibleObjectData : ExtraParameter
    {
        public const int MinSections        = 0;
        public const int DefaultNumSections = 2;
        public const int MaxSections        = 3;

        // "Tension" => [0,10], increments of 0.1
        public const float MinTension     = 0.0f;
        public const float DefaultTension = 1.0f;
        public const float MaxTension     = 10.0f;

        // "Drag" => [0,10], increments of 0.1
        public const float MinAirFriction     = 0.0f;
        public const float DefaultAirFriction = 2.0f;
        public const float MaxAirFriction     = 10.0f;

        // "Gravity" = [-10,10], increments of 0.1
        public const float MinGravity     = -10.0f;
        public const float DefaultGravity = 0.3f;
        public const float MaxGravity     = 10.0f;

        // "Wind" = [0,10], increments of 0.1
        public const float MinWindSensitivity     = 0.0f;
        public const float DefaultWindSensitivity = 0.0f;
        public const float MaxWindSensitivity     = 10.0f;

        // I'll explain later...
        public const float MaxInternalTensionForce = 0.99f;

        public const float DefaultLength = 1.0f;
        public const bool DefaultUsingCollisionSphere = false;
        public const bool DefaultRenderingCollisionSphere = false;

        /// <summary>
        /// 2^n = number of simulated sections
        /// </summary>
        public Int32 SimulateLod { get => _simulateLod; set => Mathf.Clamp(value, MinSections, MaxSections); }
        private Int32 _simulateLod;

        public float Gravity { get => _gravity; set => Mathf.Clamp(value, MinGravity, MaxGravity); }
        private float _gravity;

        /// <summary>
        /// Higher is more stable, but too much looks like it's underwater
        /// </summary>
        public float AirFriction { get => _airFriction; set => Mathf.Clamp(value, MinAirFriction, MaxAirFriction); }
        private float _airFriction;

        /// <summary>
        /// Interacts with tension, air friction, and gravity
        /// </summary>
        public float WindSensitivity { get => _windSensitivity; set => Mathf.Clamp(value, MinWindSensitivity, MaxWindSensitivity); }
        private float _windSensitivity;

        /// <summary>
        /// Interacts in complex ways with other parameters
        /// </summary>
        public float Tension { get => _tension; set => Mathf.Clamp(value, MinTension, MaxTension); }
        private float _tension;

        /// <summary>
        /// Custom user-defined force vector
        /// </summary>
        public Vector3 UserForce { get; set; }

        public FlexibleObjectData()
        {
            _simulateLod     = DefaultNumSections;
            _gravity         = DefaultGravity;
            _airFriction     = DefaultAirFriction;
            _windSensitivity = DefaultWindSensitivity;
            _tension         = DefaultTension;
            UserForce        = Vector3.zero;
            ParameterType    = ExtraParameterType.Flexible;
        }

        public override string ToString()
        {
            return $"SimulateLod: {SimulateLod}, Gravity: {Gravity}, AirFriction: {AirFriction}, WindSensitivity: {WindSensitivity}, Tension: {Tension}, UserForce: {UserForce}";
        }
    }

    public class SculptParams : ExtraParameter
    {
        public static readonly Guid DefaultSculptTextureId = Guid.Parse("be293869-d0d9-0a69-5989-ad27f1946fd4"); // Old inverted texture: "7595d345-a24c-e7ef-f0bd-78793792133e"
        public Guid SculptTextureId { get; set; }
        public SculptType SculptType { get; set; }
        public SculptFlags SculptFlags { get; set; }

        public SculptParams()
        {
            SculptTextureId = DefaultSculptTextureId;
            SculptType = SculptType.Sphere;
            SculptFlags = 0;
            ParameterType = ExtraParameterType.Sculpt;
        }

        public override string ToString()
        {
            return $"SculptTextureId: {SculptTextureId}, SculptType: {SculptType}, SculptFlags: {SculptFlags}";
        }
    }

    public class LightImageParams : ExtraParameter
    {
        public Guid LightTextureId { get; set; }
        public Vector3 Params { get; set; }

        public bool IsLightSpotLight => LightTextureId == Guid.Empty;

        public LightImageParams()
        {
            Params = new Vector3(Mathf.PI * 0.5f, 0f, 0f);
            ParameterType = ExtraParameterType.LightImage;
        }

        public override string ToString()
        {
            return $"LightTextureId: {LightTextureId}, Params: {Params}";
        }
    }

    [Flags]
    public enum ExtendedMeshFlags : UInt32
    {
        AnimatedMeshEnabled = 1
    }

    public class ExtendedMeshParams : ExtraParameter
    {
        public ExtendedMeshFlags Flags { get; set; }

        public ExtendedMeshParams()
        {
            Flags = 0;
            ParameterType = ExtraParameterType.ExtendedMesh;
        }

        public override string ToString()
        {
            return $"Flags: {Flags}";
        }
    }

}
