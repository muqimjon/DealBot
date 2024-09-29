namespace DealBot.Domain.Enums;

public enum States
{
    None,
    Restart,
    WaitingForSelectLanguage,
    WaitingForSelectMenu,
    WaitingForSendPhoneNumber,
    WaitingForSelecCustomertMenu,
    WaitingForSubscribeToChannel,
    WaitingForSelectCardOption,
    WaitingForSelectAddressOption,
    WaitingForSelectStoreContactOption,
    WaitingForSendComment,
    WaitingForSelectCustomerSettings,
    WaitingForSelectMenuChangeCustomerInfo,
    WaitingForFirstSelectLanguage,
    WaitingForSendFirstName,
    WaitingForSendLastName,
    WaitingForSendEmail,
    WaitingForSelectGender,
    WaitingForSelectDateOfBirth
}