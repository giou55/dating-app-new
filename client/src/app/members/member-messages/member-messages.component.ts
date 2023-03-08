import { ChangeDetectionStrategy, Component, Input, OnInit, ViewChild } from '@angular/core';
import { NgForm } from '@angular/forms';
import { Message } from 'src/app/_models/message';
import { MessageService } from 'src/app/_services/message.service';

@Component({
  selector: 'app-member-messages',
  templateUrl: './member-messages.component.html',
  styleUrls: ['./member-messages.component.css']
})
export class MemberMessagesComponent implements OnInit {
  @ViewChild('messageForm') messageForm?: NgForm;
  @Input() username?: string;
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

  sendMessage() {
    if (!this.username) return;
    this.loading = true;
      this.messageService.sendMessage(this.username, this.messageContent)
        .then(() => {
        this.messageForm?.reset();
        })
        .finally(() => this.loading = false);
  }

}
