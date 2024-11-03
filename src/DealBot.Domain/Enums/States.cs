﻿namespace DealBot.Domain.Enums;

public enum States
{
    None,
    Restart,
    WaitingForSelectLanguage,
    WaitingForSendPhoneNumber,
    WaitingForSelectMenu,
    WaitingForSubscribeToChannel,
    WaitingForSelectCardOption,
    WaitingForSelectAddressOption,
    WaitingForSelectStoreContactOption,
    WaitingForSendComment,
    WaitingForSelectSettings,
    WaitingForSelectUserInfo,
    WaitingForFirstSelectLanguage,
    WaitingForSendFirstName,
    WaitingForSendLastName,
    WaitingForSendEmail,
    WaitingForSelectGender,
    CommentReceived,
    WaitingForSelectDateOfBirth,
    WaitingForSelectDateOfBirthYear1,
    WaitingForSelectDateOfBirthYear2,
    WaitingForSelectDateOfBirthYear3,
    WaitingForSelectDateOfBirthYear4,
    WaitingForSelectDateOfBirthYear5,
    WaitingForSelectDateOfBirthMonth,
    WaitingForSelectDateOfBirthDay,
    WaitingForSelectCompanySettings,
    WaitingForSendName,
    WaitingForSendCompanyImage,
    WaitingForSendUserId,
    WaitingForSelectUserMenu,
    WaitingForSendProductPrice,
    WaitingForSendSalesAmount,
    WaitingForSendMessage,
    WaitingForSelectTransaction,
    WaitingForSendMessageToDeveloper,
    WaitingForConfirmation,
    CustomersList,
    WaitingForSendDescription,
    WaitingForSendMiniAppUrl,
    WaitingForSendCompanyEmail,
    WaitingForSendCompanyPhoneNumber,
    WaitingForSendWebsite,
    WaitingForSendChannel,
    EmployeesList,
    WaitingForSelectCardType,
    WaitingForSelectCashbackQuantityPremium,
    WaitingForSelectCashbackQuantitySimple,
}