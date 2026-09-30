namespace Majipro.Converter.Generator.Test.Conversions.AssignablePairs;

public class AssignablePairsTestCase
{
    public class Animal
    {
        public string Name { get; set; }
    }

    public class Dog : Animal
    {
        public bool Fetches { get; set; }
    }

    /// <summary>
    /// Covariant in its argument, so an <c>IProducer&lt;Dog&gt;</c> already is an
    /// <c>IProducer&lt;Animal&gt;</c> - the compiler says so, and nothing has to be converted.
    /// </summary>
    public interface IProducer<out T>
    {
        T Produce();
    }

    /// <summary>
    /// Contravariant in its argument, which is the same fact the other way round: an
    /// <c>IConsumer&lt;Animal&gt;</c> already is an <c>IConsumer&lt;Dog&gt;</c>.
    /// </summary>
    public interface IConsumer<in T>
    {
        void Consume(T value);
    }

    public class DogProducer : IProducer<Dog>
    {
        public Dog Produce()
        {
            return new Dog();
        }
    }

    public class AnimalConsumer : IConsumer<Animal>
    {
        public void Consume(Animal value)
        {
        }
    }
}
