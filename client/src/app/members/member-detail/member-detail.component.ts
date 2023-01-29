import { Component, OnInit, ViewChild } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { NgxGalleryAnimation, NgxGalleryImage, NgxGalleryOptions } from '@kolkov/ngx-gallery';
import { TabDirective, TabsetComponent } from 'ngx-bootstrap/tabs';
import { Member } from 'src/app/_models/member';
import { Message } from 'src/app/_models/message';
import { MembersService } from 'src/app/_services/members.service';
import { MessageService } from 'src/app/_services/message.service';
import { PresenceService } from 'src/app/_services/presence.service';

@Component({
  selector: 'app-member-detail',
  templateUrl: './member-detail.component.html',
  styleUrls: ['./member-detail.component.css']
})
export class MemberDetailComponent implements OnInit {
  // we don't have access to the View until the View has actually been created,
  // so we make memberTabs optional
  // @ViewChild('memberTabs') memberTabs?: TabsetComponent;

  // we add the static property, so instead of waiting for the view to load
  // we can specify that it is static and not dynamic
  @ViewChild('memberTabs', {static: true}) memberTabs?: TabsetComponent;

  //member: Member | undefined;
  member: Member = {} as Member;
  galleryOptions: NgxGalleryOptions[] = [];
  galleryImages: NgxGalleryImage[] = [];
  lastActiveDate: Date | undefined; 
  activeTab?: TabDirective;
  messages: Message[] = [];

  constructor(
    private route: ActivatedRoute,
    private messageService: MessageService,
    // we make it public, so that we can use the async pipe in the template
    public presenceService: PresenceService 
  ) { }

  ngOnInit(): void {
    // we no longer need to use this, because we're going to get the member from our route instead
    //this.loadMember();

    // our resolver is going to place the member here 
    this.route.data.subscribe({
      next: data => {
        this.member = data['member'];
        this.lastActiveDate = new Date(data['member'].lastActive + 'Z');
      }
    })

    // we access the queryParams of our route and it returns an observable
    this.route.queryParams.subscribe({
      next: params => {
        // this line is to make sure that we have the params before we attempt to use the params
        params['tab'] && this.selectTab(params['tab'])
      }
    })

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

  // we don't need this because we now use resolver to get the member
  // loadMember() {
  //   const username = this.route.snapshot.paramMap.get('username');
  //   if (!username) return;
  //   this.membersService.getMember(username).subscribe({
  //     next: member => {
  //       this.member = member;
  //       this.lastActiveDate = new Date(member.lastActive + 'Z');
  //       this.galleryImages = this.getImages();
  //     }
  //   })
  // }

  selectTab(heading: string) {
    // inside memberTabs we have a tabs array
    if (this.memberTabs) {
      this.memberTabs.tabs.find(x => x.heading === heading)!.active = true;
    }
  }

  loadMessages() {
    if (this.member) {
      this.messageService.getMesssageThread(this.member.userName).subscribe({
        next: messages => {
          messages.map(message => {
                message.messageSent = new Date(message.messageSent + 'Z');
                if (message.dateRead) {
                  message.dateRead = new Date(message.dateRead + 'Z');
                }
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
