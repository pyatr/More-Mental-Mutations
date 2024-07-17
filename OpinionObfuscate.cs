using Occult.Engine.CodeGeneration;
using System.CodeDom.Compiler;
using XRL.World;
using XRL.World.AI;

namespace MoreMentalMutations.Opinions
{
    [GenerateSerializationPartial]
    public class OpinionObfuscate : IOpinionSubject
    {
        [GeneratedCode("SerializationPartialsGenerator", "1.0.0.0")]
        public override bool WantFieldReflection
        {
            get
            {
                return false;
            }
        }

        [GeneratedCode("SerializationPartialsGenerator", "1.0.0.0")]
        public override void Write(SerializationWriter Writer)
        {
            Writer.Write(this.Magnitude);
            Writer.WriteOptimized(this.Time);
        }

        [GeneratedCode("SerializationPartialsGenerator", "1.0.0.0")]
        public override void Read(SerializationReader Reader)
        {
            this.Magnitude = Reader.ReadSingle();
            this.Time = Reader.ReadOptimizedInt64();
        }

        public override int BaseValue
        {
            get
            {
                return 0;
            }
        }

        public override float Limit
        {
            get
            {
                return 0;
            }
        }

        public override string GetText(GameObject Actor)
        {
            return $"{Actor.ShortDisplayName} suddenly disappeared.";
        }
    }
}