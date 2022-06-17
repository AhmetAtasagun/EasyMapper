namespace EasyMapper
{
    public class MapOptions
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

        public MapOptions() { }

        public MapOptions(GenerationLevel generationLevel, params string[] ignoreFields)
        {
            GenerationLevel = generationLevel;
            IgnoreFields = ignoreFields;
        }

        public MapOptions GetDefaultOptions() => this;

        public MapOptions GetSecondLevelAndNonIgnoreOptions()
        {
            GenerationLevel = GenerationLevel.Second;
            return this;
        }

        public MapOptions GetThirdLevelAndNonIgnoreOptions()
        {
            GenerationLevel = GenerationLevel.Third;
            return this;
        }
    }
}
