// Login API Helper
window.ValyanAuth = {
    login: async function (username, password, rememberMe, resetPasswordOnFirstLogin) {
        try {
   const response = await fetch('/api/authentication/login', {
     method: 'POST',
       headers: {
      'Content-Type': 'application/json'
       },
        credentials: 'include', // IMPORTANT: Include cookies
           body: JSON.stringify({
        username,
    password,
            rememberMe,
        resetPasswordOnFirstLogin
})
            });

  if (response.ok) {
          const result = await response.json();
                return { success: true, data: result };
      } else {
       const error = await response.json();
                return { success: false, message: error.message || 'Login failed' };
   }
        } catch (error) {
        return { success: false, message: error.message };
        }
    },

    logout: async function () {
        try {
            await fetch('/api/authentication/logout', {
                method: 'POST',
                credentials: 'include'
            });
            return { success: true };
        } catch (error) {
 return { success: false, message: error.message };
        }
    }
};
