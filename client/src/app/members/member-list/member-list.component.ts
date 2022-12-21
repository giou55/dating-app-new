import { Component, OnInit } from '@angular/core';
import { Observable } from 'rxjs';
import { Member } from 'src/app/_models/member';
import { MembersService } from 'src/app/_services/members.service';

@Component({
  selector: 'app-member-list',
  templateUrl: './member-list.component.html',
  styleUrls: ['./member-list.component.css']
})
export class MemberListComponent implements OnInit {
  //members: Member[] = [];

  // we make members an observable of type Member array
  // and remove loadMembers method
  members$: Observable<Member[]> | undefined;

  constructor(private membersService: MembersService) { }

  ngOnInit(): void {
    //this.loadMembers();
    this.members$ = this.membersService.getMembers();
  }

  // loadMembers() {
  //   this.membersService.getMembers().subscribe({
  //     next: members => this.members = members
  //   })
  // }

}
