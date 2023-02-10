import { ChangeDetectionStrategy, Component, Input, OnInit, ViewChild } from '@angular/core';
import { NgForm } from '@angular/forms';
import { Message } from 'src/app/_models/message';
import { MessageService } from 'src/app/_services/message.service';

@Component({
  //changeDetection: ChangeDetectionStrategy.OnPush,
  selector: 'app-member-messages',
  templateUrl: './member-messages.component.html',
  styleUrls: ['./member-messages.component.css']
})
export class MemberMessagesComponent implements OnInit {
  @ViewChild('messageForm') messageForm?: NgForm;
  @Input() username?: string;
  // @Input() messages: Message[] = [];
  messageContent = '';
  messages: Message[] = [];
  loading = false;

  constructor(private messageService: MessageService) { }

  ngOnInit(): void {
    this.loadMessages();
  }

  loadMessages() {
    this.messageService.messageThread$.subscribe({
      next: messages => {
        messages.map(m => {
          m.messageSent = new Date(m.messageSent);
          m.dateRead = m.dateRead ? new Date(m.dateRead) : undefined;
        });
        this.messages = messages;
      }
    })
  }

  // sendMessage method is different now with SignalR,
  // we're not subscribing anymore to this.messageService.sendMessage
  // because now it's returning a promise

  // sendMessage() {
  //   if (!this.username) return;
  //   this.messageService.sendMessage(this.username, this.messageContent).subscribe({
  //     next: message => {
  //       message.messageSent = new Date(message.messageSent + 'Z');
  //         this.messages.push(message);
  //         this.messageForm?.reset();
  //     }
  //   })
  // }

  // this.messageService.sendMessage is returning a promise
  sendMessage() {
    if (!this.username) return;
    this.loading = true;
      this.messageService.sendMessage(this.username, this.messageContent)
        .then(() => {
        // we don't need to do anything with the message we get back,
        // because messageThread$ observable from MessageService class is handling that
        this.messageForm?.reset();
        })
        .finally(() => this.loading = false);
  }

}
