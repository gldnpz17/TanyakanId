import styled from "styled-components";
import { Link } from "react-router-dom";
import { useState } from "react";

import AuthCard, { FormTitle, FinePrint, AuthForm, AuthButton } from "../components/auth-card";
import LabeledInput from "../components/labeled-input";

import { signUp, logIn } from "../api/authentication";

const PageContainer = styled.div`
    min-height: 90vh;

    background-color: #3399D2; /* Subject to change */

    display: flex;
    align-items: center;
`;

const ExplanationPart = styled.div`
    flex-grow: 1;

    color: white;
`;

const EntryPart = styled.div`
    flex-grow: 0;
    flex-shrink: 0;
`;

const CatchPhrase = styled.h1`
    font-weight: normal;
    font-family: "Red Hat Display", sans-serif;

    &:first-of-type {
        font-weight: bold;
    }
`;

const CompanionText = styled.p`
    font-family: "Red Hat Text", sans-serif;
`;

const SignUpCard = () => {
    // States for the controlled inputs
    const [usernameValue, setUsernameValue] = useState("");
    const [emailValue, setEmailValue] = useState("");
    const [passwordValue, setPasswordValue] = useState("");
    const [repeatPasswordValue, setRepeatPasswordValue] = useState("");

    // Some checking before attempting to submit the form
    const [usernameAvailability, setUsernameAvailability] = useState(null);
    const [doesPasswordMatch, setPasswordMatch] = useState(null);

    // Now-already-dispatched timeout
    const [runningTimeout, assignRunningTimeout] = useState(null);

    async function checkUsernameAvailability(uname) {
        let avail; // Declaring here so that it's accessible within the entire try-catch-finally blocks

        try {
            const response = await fetch(`api/users/check-username-availability/${uname}`);
            avail = await response.json();
        }
        catch (err) {
            avail = {
                username: uname,
                isAvailable: null,
                error: err
            };
            console.error(err);
        }
        finally {
            return avail; 
        }
    }

    function onUsernameChange(ev) {
        const value = ev.target.value;
        setUsernameValue(value);

        if (runningTimeout) {
            clearTimeout(runningTimeout);
            assignRunningTimeout(null);
        }

        if (ev.target.validity.valid) {
            setUsernameAvailability("loading");

            assignRunningTimeout(
                setTimeout(async () => {
                    const avail = await checkUsernameAvailability(value);
                    setUsernameAvailability(avail);
                }, 500)
            );
        }
        else {
            setUsernameAvailability(null);
        }
    }

    const usernameAvailabilityMessage = () => {
        switch (usernameAvailability) {
            case null:
                return null;

            case "loading":
                return {
                    color: "gray",
                    message: `Memeriksa ketersediaan username...`
                };
        }

        switch (usernameAvailability.isAvailable) {
            case true:
                return {
                    color: "green",
                    message: <>Selamat, <b>{usernameAvailability.username}</b> tersedia!</>
                };

            case false:
                return {
                    color: "red",
                    message: <>Maaf, <b>{usernameAvailability.username}</b> telah digunakan.</>
                };

            case null:
            default:
                return {
                    color: "red",
                    message: <>Terjadi eror dalam memeriksa ketersediaan username <b>{usernameAvailability.username}</b>.</>
                }
        }
    }


    function onRepeatPasswordChange({ target: { value } }) {
        setRepeatPasswordValue(value);
        setPasswordMatch(passwordValue === value);
    }

    async function doSignUp(ev) {
        ev.preventDefault();
        console.log(ev.target);

        // We need this because setState is async so we can't rely on that for getting the current value
        let _usernameAvailability = usernameAvailability;

        if (_usernameAvailability === null || _usernameAvailability === "loading") {
            _usernameAvailability = await checkUsernameAvailability(usernameValue);
            setUsernameAvailability(_usernameAvailability);
        }

        //#region -- A set of early returns in case the form is still invalid

        if (!_usernameAvailability.isAvailable) {
            alert(`The username "${usernameValue}" is already taken. Please try another!`)
            return;
        }

        if (doesPasswordMatch === null) {
            setPasswordMatch(passwordValue === repeatPasswordValue);
        }

        if (!doesPasswordMatch) {
            alert("Password doesn't match!");
            return;
        }

        //#endregion

        // If the code reached this point, we assume that the form is already valid
        try {
            await signUp(usernameValue, passwordValue, emailValue);
            await logIn(usernameValue, passwordValue);
            window.location.reload();
        }
        catch (err) {
            console.error(err);
            alert("There is some error happening while signing you up");
        }
    }

    return (
        <AuthCard>
            <FormTitle>Buat akun baru</FormTitle>
            <FinePrint>Sudah pernah mendaftar? <Link to="/masuk">Klik di sini untuk masuk</Link>.</FinePrint>
            <AuthForm onSubmit={doSignUp}>
                <LabeledInput
                    name="email" title="Alamat e-mail" type="email" required autoFocus
                    value={emailValue} onChange={e => { setEmailValue(e.target.value) }}
                />
                <LabeledInput
                    name="username" title="Username (alfanumerik dan angka saja)" type="text" required autoComplete="username" pattern="[a-zA-Z0-9]+"
                    value={usernameValue} onChange={onUsernameChange}
                    customError={usernameAvailabilityMessage()}
                />
                <LabeledInput
                    name="password" title="Kata sandi" type="password" required autoComplete="new-password"
                    value={passwordValue} onChange={e => { setPasswordValue(e.target.value) }}
                />
                <LabeledInput
                    name="repeat-password" title="Ulangi kata sandi" type="password" autoComplete="new-password"
                    value={repeatPasswordValue} onChange={onRepeatPasswordChange}
                    customError={doesPasswordMatch === false ? {
                        color: "red",
                        message: "Kata sandi tidak sesuai"
                    } : null}
                />
                <AuthButton backgroundColor="#23CC20" onClick={e => { /*setSubmitAttempt(true)*/ }}>Daftar</AuthButton>
            </AuthForm>
        </AuthCard>
    )
}

const SignUpPage = () => (
    <PageContainer>
        <ExplanationPart>
            <CatchPhrase>Ingin berkontribusi?</CatchPhrase>
            <CatchPhrase>Silakan membuat akun terlebih dahulu.</CatchPhrase>
            <CompanionText>
                Data yang anda masukkan akan dijaga sesuai dengan <Link to="/privasi">kebijakan privasi kami</Link>.
                Dengan berkontribusi, Anda turut serta dalam mengembangkan koleksi yang dimiliki oleh <b>gimana.id</b>.
            </CompanionText>
        </ExplanationPart>
        <EntryPart>
            <SignUpCard />
        </EntryPart>
    </PageContainer>
);

export default SignUpPage;