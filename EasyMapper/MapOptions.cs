namespace EasyMapper
{
    public sealed class MapOptions
    {
        /// <summary>
        /// <see href="EN"/> : Indicates which sub-level should be descended from the sub-assets. | 
        /// <see href="TR"/> : Alt varlıklardan, kaçıncı alt seviyeye kadar inilmesi gerektiğini belirtir.
        /// </summary>
        public GenerationLevel GenerationLevel { get; set; } = GenerationLevel.First;

        /// <summary>
        /// <see href="EN"/> : Fields that should not match. | 
        /// <see href="TR"/> : Eşleşmemesi gereken alanlar.
        /// </summary>
        public string[] IgnoreFields { get; set; } = new string[0];

        public MapOptions(GenerationLevel generationLevel, params string[] ignoreFields)
        {
            GenerationLevel = generationLevel;
            IgnoreFields = ignoreFields;
        }

        //public static MapOptions GetDefaultOptions()
        //{
        //    GenerationLevel = GenerationLevel.First;
        //    IgnoreFields = new string[0];
        //    return default;
        //}
    }

    public class MapOption
    {
        public static MapOptions GetDefaultOptions()
            => new MapOptions(GenerationLevel.First, new string[0]);
    }
}
