import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { HubConnection, HubConnectionBuilder } from '@microsoft/signalr';
import { BehaviorSubject, take } from 'rxjs';
import { environment } from 'src/environments/environment';
import { Group } from '../_models/group';
import { Message } from '../_models/message';
import { User } from '../_models/user';
import { getPaginatedResult, getPaginationHeaders } from './paginationHelper';

@Injectable({
  providedIn: 'root'
})
export class MessageService {
  baseUrl = environment.apiUrl;
  hubUrl = environment.hubUrl;
  private hubConnection?: HubConnection;
  private messageThreadSource = new BehaviorSubject<Message[]>([]); // initialize with an empty array
  
  // we create an observable from messageThreadSource,
  // so we'll have something to subscribe to from our components
  messageThread$ = this.messageThreadSource.asObservable();

  constructor(private http:HttpClient) { }

  createHubConnection(user: User, otherUsername: string) {
    // the otherUsername we're going to get from the member-detail component,
    // which will get it from the route parameter  

    this.hubConnection = new HubConnectionBuilder()
      // 'message' needs to match the name of our endpoint
      // when we mapped the hub inside Program.cs file in our api
      .withUrl(this.hubUrl + 'message?user=' + otherUsername, {
        accessTokenFactory: () => user.token
      })
      .withAutomaticReconnect()
      .build();

    this.hubConnection.start().catch(error => console.log(error));

    // 'ReceiveMessageTread' needs to match the name inside MessageHub.cs in our api,
    // we get the messages back from SignalR
    this.hubConnection.on('ReceiveMessageTread', messages => {
      this.messageThreadSource.next(messages);
    })

    // 'UpdatedGroup' needs to match the name inside MessageHub.cs in our api,
    // we get the group back from SignalR
    this.hubConnection.on('UpdatedGroup', (group: Group) => {
      if (group.connections.some(x => x.username === otherUsername)) {
        this.messageThread$.pipe(take(1)).subscribe({
          next: messages => {
            messages.forEach(message => {
              if (!message.dateRead) {
                message.dateRead = new Date(Date.now());
              }
            });
            this.messageThreadSource.next([...messages]);
          }
        })
      }
    })

    // 'NewMessage' needs to match the name inside MessageHub.cs in our api,
    // we get the message back from SignalR
    this.hubConnection.on('NewMessage', message => {
      // we create a new array of messages, and replace the old array with the new array
      this.messageThread$.pipe(take(1)).subscribe({
        next: messages => {
          this.messageThreadSource.next([...messages, message])
        }
      })
    })
  }

  stopHubConnection() {
    if (this.hubConnection) {
      this.hubConnection.stop();
    }
  }

  getMessages(pageNumber: number, pageSize: number, container: string) {
    let params = getPaginationHeaders(pageNumber, pageSize);
    params = params.append('Container', container);

    return getPaginatedResult<Message[]>(this.baseUrl + 'messages', params, this.http);
  }

  getMesssageThread(username: string) {
    return this.http.get<Message[]>(this.baseUrl + 'messages/thread/' + username);
  }

  sendMessageOLD(username: string, content: string) { // the method before using SignalR
    // recipientUsername and content must match what API expect to receive in the CreateMessageDto  
    return this.http.post<Message>(this.baseUrl + 'messages', {recipientUsername: username, content});
  }
  
  async sendMessage(username: string, content: string) {
    // this invokes a message on our API hub,
    // invokes method returns a promise,
    // 'SendMessage' needs to match the method name inside MessageHub.cs in our api,
    // recipientUsername and content must match what API expect to receive in the CreateMessageDto  
    return this.hubConnection?.invoke('SendMessage', {recipientUsername: username, content})
      .catch(error => console.log(error));
  }

  deleteMessage(id: number) {
    return this.http.delete(this.baseUrl + 'messages/' + id);
  }
}
