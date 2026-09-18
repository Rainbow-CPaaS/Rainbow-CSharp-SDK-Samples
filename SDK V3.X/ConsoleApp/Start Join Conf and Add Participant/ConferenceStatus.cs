using Rainbow.Model;


namespace StartJoinConfAndAddParticipant
{
    internal class ConferenceStatus
    {
        public Boolean BubbleCreated { get; set; } = false;

        public Boolean ConferenceInProgress { get; set; } = false;

        public Boolean ConferenceWithOperatorActive { get; set; } = false;

        public Boolean ConferenceDelegatedToOperator { get; set; } = false;

        public Contact? Operator { get; set; } = null;

        public Bubble? Bubble { get; set; } = null;

        public List<String> PhoneNumbersToHaveInConf { get; set; } = []; // all phone numbers to have in conf

        public List<String> PhoneNumbersInProgress { get; set; } = []; // all phone numbers which are currently added in conf

        public List<String> PhoneNumbersAlreadyInConference { get; set; } = []; // all phone numbers already managed in conf

        public List<String> PhoneNumbersAlreadyActiveInConference { get; set; } = []; // all phone numbers already active in conf

        public void Reset()
        {
            BubbleCreated = false;
            ConferenceInProgress = false;
            ConferenceWithOperatorActive = false;
            ConferenceDelegatedToOperator = false;
            Operator = null;
            Bubble = null;

            PhoneNumbersToHaveInConf.Clear();
            PhoneNumbersInProgress.Clear();
            PhoneNumbersAlreadyInConference.Clear();
            PhoneNumbersAlreadyActiveInConference.Clear();
        }
    }
}
