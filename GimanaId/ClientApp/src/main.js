import { BrowserRouter, Switch, Route } from "react-router-dom";
import { useState, useEffect } from "react";
import { createGlobalStyle } from "styled-components";

// Authentication-related
import { AuthProvider } from "./utils/auth-context";
import { getCurrentUserInfo } from "./utils/authentication";

// Common page elements
import Header from "./components/header";

// Pages for each route
import HomePage from "./pages/home";
import RegisterPage from "./pages/sign-up";
import LoginPage from "./pages/log-in";
import AuthExperimentPage from "./pages/authentication-experiment";

// Main app entry point
const App = () => {
    const [userInfo, setUserInfo] = useState(null);

    // Populate user info on authenticated
    async function checkAuthenticatedStatus() {
        try {
            const userInfo = await getCurrentUserInfo();

            setUserInfo({
                isLoggedIn: true,
                username: userInfo.username
            });
        }
        catch (e) {
            if (e.status === 401) {
                setUserInfo({
                    isLoggedIn: false
                })
            }
            else {
                console.error(e);
                alert("Error making userInfo request to the server.");
            }
        }
    }

    useEffect(() => {
        checkAuthenticatedStatus();
    }, []);

    return (
        <BrowserRouter>
            <AuthProvider value={userInfo}>
                <Header />
                <Switch>
                    <Route exact path="/" component={HomePage} />
                    <Route path="/daftar" component={RegisterPage} />
                    <Route path="/masuk"  component={LoginPage} />

                    {/* For some fun things */}
                    <Route path="/authentication-experiment" component={AuthExperimentPage} />

                    {/* 404 page */}
                    <Route>Page not found.</Route>
                </Switch>
                {process.env.NODE_ENV === "development" ? <DebuggingOutlines /> : null}
            </AuthProvider>
        </BrowserRouter>
    )
};

export default App;

// Show layout outlines on dev environment
const DebuggingOutlines = createGlobalStyle`
    * {
        outline: 1px solid rgb(255 0 0 / 0.25);
    }
`;