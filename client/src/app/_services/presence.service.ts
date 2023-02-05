import { Injectable } from '@angular/core';
import { Router } from '@angular/router';
import { HubConnection } from '@microsoft/signalr';
import { HubConnectionBuilder } from '@microsoft/signalr/dist/esm/HubConnectionBuilder';
import { ToastrService } from 'ngx-toastr';
import { BehaviorSubject, take } from 'rxjs';
import { environment } from 'src/environments/environment';
import { User } from '../_models/user';

@Injectable({
  providedIn: 'root'
})

// we want to maintain a permanent connection to the hub when user are logged in,
// and we're going to create this connection in setCurrentUser method inside AccountService class

export class PresenceService {
  hubUrl = environment.hubUrl;
  private hubConnection?: HubConnection;
  private onlineUsersSource = new BehaviorSubject<string[]>([]); // initialize with an empty array 
  
  // we create an observable from onlineUsersSource,
  // so we'll have something to subscribe to from our components
  onlineUsers$ = this.onlineUsersSource.asObservable();

  constructor(private toastr: ToastrService, private router: Router) { }

  createHubConnection(user: User) {
    this.hubConnection = new HubConnectionBuilder()
      // 'presence' needs to match the name of our endpoint
      // when we mapped the hub inside Program.cs file in our api
      .withUrl(this.hubUrl + 'presence', {
        accessTokenFactory: () => user.token 
      })
      .withAutomaticReconnect()
      .build();

    // start() returns a promise that resolves when the connection has been
    // successfully established or rejects with an error
    this.hubConnection.start().catch(error => console.log(error));

    // 'UserIsOnline' needs to match the name inside PresenceHub.cs in our api,
    // we get the username back from SignalR
    this.hubConnection.on('UserIsOnline', username => {
      this.toastr.info(username + ' has connected');
    })

    // 'UserIsOffline' needs to match the name inside PresenceHub.cs in our api,
    // we get the username back from SignalR
    this.hubConnection.on('UserIsOffline', username => {
      this.toastr.warning(username + ' has disconnected');
    })

    // 'GetOnlineUsers' needs to match the name inside PresenceHub.cs in our api,
    // we get the usernames back from SignalR
    this.hubConnection.on('GetOnlineUsers', usernames => {
      this.onlineUsersSource.next(usernames);
    })

    // 'NewMessageReceived' needs to match the name inside MessageHub.cs in our api,
    // we get the {username, knownAs} object back from SignalR
    this.hubConnection.on('NewMessageReceived', ({username, knownAs}) => {
      this.toastr.info(knownAs + ' has sent you a new message! Click me to see it')
        .onTap
        .pipe(take(1))
        .subscribe({
          next: () => this.router.navigateByUrl('/members/' + username + '?tab=Messages')
        })
    })
  }

  stopHubConnection() {
    this.hubConnection?.stop().catch(error => console.log(error));
  }
}
