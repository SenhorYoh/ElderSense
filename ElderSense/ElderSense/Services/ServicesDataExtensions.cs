namespace ElderSense.Services
{
    /// <summary>
    /// Métodos de extensão para conversão de datas guardadas em UTC na base de dados
    /// </summary>
    public static class DataExtensions
    {
        /// <summary>
        /// Converte uma data guardada em UTC para a hora local de Portugal,
        /// tendo em conta o horário de verão
        /// </summary>
        public static DateTime ParaHoraPortugal(this DateTime dataUtc)
        {
            // garante que a data é tratada como UTC antes de converter
            var utc = DateTime.SpecifyKind(dataUtc, DateTimeKind.Utc);

            // "GMT Standard Time" é o identificador do fuso de Lisboa/Londres no Windows
            var fusoPortugal = TimeZoneInfo.FindSystemTimeZoneById("GMT Standard Time");

            return TimeZoneInfo.ConvertTimeFromUtc(utc, fusoPortugal);
        }
    }
}
