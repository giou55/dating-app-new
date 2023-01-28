import { Injectable } from '@angular/core';
import { HubConnection } from '@microsoft/signalr';
import { HubConnectionBuilder } from '@microsoft/signalr/dist/esm/HubConnectionBuilder';
import { ToastrService } from 'ngx-toastr';
import { environment } from 'src/environments/environment';
import { User } from '../_models/user';

@Injectable({
  providedIn: 'root'
})

// we want to maintain a permanent connection to the hub when user are logged in,
// and we're going to create this connection in our AccountService class

export class PresenceService {
  hubUrl = environment.hubUrl;
  private hubConnection?: HubConnection;

  constructor(private toastr: ToastrService) { }

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

    // 'UserIsOnline' needs to match the name inside PresenceHub.cs in our api
    // we get the username back from SignalR
    this.hubConnection.on('UserIsOnline', username => {
      this.toastr.info(username + ' has connected');
    })

    // 'UserIsOffline' needs to match the name inside PresenceHub.cs in our api
    // we get the username back from SignalR
    this.hubConnection.on('UserIsOffline', username => {
      this.toastr.warning(username + ' has disconnected');
    })
  }

  stopHubConnection() {
    this.hubConnection?.stop().catch(error => console.log(error));
  }
}
