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
        private string[] IgnoreFields { get; set; } = new string[0];

        public MapOptions(GenerationLevel generationLevel, params string[] ignoreFields)
        {
            GenerationLevel = generationLevel;
            IgnoreFields = ignoreFields;
        }
        public MapOptions() { }

        private void DefaultOptions(out GenerationLevel level, out string[] ignoreds)
        {
            level = GenerationLevel.First;
            ignoreds = new string[0];
        }

        //public static MapOptions GetDefaultOptions()
        //{
        //    var mo = new MapOptions();
        //    DefaultOptions(ref mo.GenerationLevel, ref out mo.IgnoreFields);
        //    return mo;
        //}
    }

    public class MapOption
    {
        public static MapOptions GetDefaultOptions()
            => new MapOptions(GenerationLevel.First, new string[0]);

        public static MapOptions GetThirdLevelAndNonIgnoreOptions()
            => new MapOptions(GenerationLevel.Third, new string[0]);
    }
}
