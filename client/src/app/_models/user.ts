// this interface is used to store user data in local storage
// when user logs in or register

export interface User {
    username: string;
    token: string;
    photoUrl: string;
    knownAs: string;
    gender: string
}