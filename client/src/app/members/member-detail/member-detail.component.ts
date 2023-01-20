import { Component, OnInit, ViewChild } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { NgxGalleryAnimation, NgxGalleryImage, NgxGalleryOptions } from '@kolkov/ngx-gallery';
import { TabDirective, TabsetComponent } from 'ngx-bootstrap/tabs';
import { Member } from 'src/app/_models/member';
import { Message } from 'src/app/_models/message';
import { MembersService } from 'src/app/_services/members.service';
import { MessageService } from 'src/app/_services/message.service';

@Component({
  selector: 'app-member-detail',
  templateUrl: './member-detail.component.html',
  styleUrls: ['./member-detail.component.css']
})
export class MemberDetailComponent implements OnInit {
  // we don't have access to the View until the View has actually been created,
  // so we make memberTabs optional
  @ViewChild('memberTabs') memberTabs?: TabsetComponent;

  member: Member | undefined;
  galleryOptions: NgxGalleryOptions[] = [];
  galleryImages: NgxGalleryImage[] = [];
  lastActiveDate: Date | undefined; 
  activeTab?: TabDirective;
  messages: Message[] = [];

  constructor(
    private membersService: MembersService, 
    private route: ActivatedRoute,
    private messageService: MessageService) { }

  ngOnInit(): void {
    this.loadMember();

    this.galleryOptions = [
      {
        width: '500px',
        height: '500px',
        imagePercent: 100,
        thumbnailsColumns: 4,
        imageAnimation: NgxGalleryAnimation.Slide,
        preview: false
      }
    ];

    this.galleryImages = this.getImages();
  }
  
  getImages() {
    if (!this.member) return [];
    const imageUrls = [];
    for (const photo of this.member.photos) {
      imageUrls.push({
        small: photo.url,
        medium: photo.url,
        big: photo.url
      })
    }
    return imageUrls;
  }

  loadMember() {
    const username = this.route.snapshot.paramMap.get('username');
    if (!username) return;
    this.membersService.getMember(username).subscribe({
      next: member => {
        this.member = member;
        this.lastActiveDate = new Date(member.lastActive + 'Z');
        this.galleryImages = this.getImages();
      }
    })
  }

  selectTab(heading: string) {
    if (this.memberTabs) {
      // inside memberTabs we have a tabs array
      this.memberTabs.tabs.find(x => x.heading === heading)!.active = true;
    }
  }

  loadMessages() {
    if (this.member) {
      this.messageService.getMesssageThread(this.member.userName).subscribe({
        next: messages => {
          messages.map(message => {
                message.messageSent = new Date(message.messageSent + 'Z');
                message.dateRead = new Date(message.dateRead + 'Z');
          });
          this.messages = messages;

        }
      })
    }
  }

  onTabActivated(data: TabDirective) {
    this.activeTab = data;
    // if we are on the messages tab, we want to load the messages
    if (this.activeTab.heading === 'Messages') {
      this.loadMessages();
    }
  }

}
