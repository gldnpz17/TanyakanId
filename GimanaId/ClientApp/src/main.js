import { BrowserRouter, Switch, Route } from "react-router-dom";
import { useState, useEffect } from "react";
import { createGlobalStyle } from "styled-components";

// Authentication-related
import { AuthProvider } from "./components/auth-context";
import { getCurrentUserInfo } from "./api/authentication";

// Router components used based on the user authentication status
import { PrivateRoute, PublicOnlyRoute } from "./components/routes";

// Common page elements
import Header from "./components/header";
import Footer from "./components/footer";

//#region Page components for each route
import HomePage from "./pages/home";
import ArticleListingPage from "./pages/article-listing";

import RegisterPage from "./pages/sign-up";
import LoginPage from "./pages/log-in";

import ArticleViewerPage from "./pages/article-viewer";
import ArticleEditorPage from "./pages/article-editor";

import UserPage from "./pages/user/user-page";

import NotFoundPage from "./pages/not-found";
//#endregion

// Declare main app/website routes
const MainRoutes = () => (
    <Switch>
        <Route exact path="/" component={HomePage} />

        <PublicOnlyRoute path="/daftar" component={RegisterPage} />
        <PublicOnlyRoute path="/masuk" component={LoginPage} />

        <PrivateRoute path="/artikel/baru"><ArticleEditorPage mode="new" /></PrivateRoute>
        <PrivateRoute path="/artikel/:articleGuid/edit"><ArticleEditorPage mode="edit" /></PrivateRoute>
        <Route path="/artikel/:articleGuid" component={ArticleViewerPage} />

        <Route path="/artikel" component={ArticleListingPage} />

        <PrivateRoute path="/anda" component={UserPage} />

        <Route component={NotFoundPage} />
    </Switch>
);

// Main app entry point
const App = () => {
    const [userInfo, setUserInfo] = useState(null);

    // Populate user info on authenticated
    async function checkAuthenticatedStatus() {
        try {
            const userInfo = await getCurrentUserInfo();

            setUserInfo({
                isLoggedIn: true,
                ...userInfo
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

    const authContextValues = {
        userInfo: userInfo,
        refreshUserInfo: () => {
            checkAuthenticatedStatus();
        }
    }

    return (
        <BrowserRouter>
            <AuthProvider value={authContextValues}>
                <Header />
                <MainRoutes />
                <Footer />
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