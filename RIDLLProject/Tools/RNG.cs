namespace RelationsInspector
{
	public class RNG
	{
		System.Random rng;

		public RNG( int seed )
		{
			rng = new System.Random( seed );
		}

		public float Range( float min = 0f, float max = 1f )
		{
			float range = max - min;
			float rndValue = (float) rng.NextDouble();
			return min + range * rndValue;
		}
	}
}
