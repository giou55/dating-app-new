import { Component, OnInit } from '@angular/core';
import { Observable, take } from 'rxjs';
import { Member } from 'src/app/_models/member';
import { Pagination } from 'src/app/_models/pagination';
import { User } from 'src/app/_models/user';
import { UserParams } from 'src/app/_models/userParams';
import { AccountService } from 'src/app/_services/account.service';
import { MembersService } from 'src/app/_services/members.service';

@Component({
  selector: 'app-member-list',
  templateUrl: './member-list.component.html',
  styleUrls: ['./member-list.component.css']
})
export class MemberListComponent implements OnInit {
  // we do not need members as an observable of type Member array
  //members$: Observable<Member[]> | undefined;

  members: Member[] = [];
  pagination: Pagination | undefined;
  userParams: UserParams | undefined;
  genderList = [{value: 'male', display: '΄Ανδρες'}, {value: 'female', display: 'Γυναίκες'}];

  constructor(private membersService: MembersService) {
    this.userParams = this.membersService.getUserParams();
  }

  ngOnInit(): void {
    // getMembers method now does not return all members
    // because we added pagination and
    // getMembers method now need parameters 
    //this.members$ = this.membersService.getMembers();
    this.loadMembers();
  }

  loadMembers() {
    if (this.userParams) {
      this.membersService.setUserParams(this.userParams);
      this.membersService.getMembers(this.userParams).subscribe({
        next: response => {
          if (response.result && response.pagination) {
            this.members = response.result;
            this.pagination = response.pagination;
          }
        }
      })
    }
  }

  // this is going to take us back to the page 1 
  // with default ages and default gender
  resetFilters() {
    // before call loadMembers method we reset userParams to default values 
    this.userParams = this.membersService.resetUserParams();
    this.loadMembers();
  }

  pageChanged(event: any) {
    if (this.userParams && this.userParams?.pageNumber !== event.page) {
      this.userParams.pageNumber = event.page;
      // the pageNumber changed, so we update the userParams
      this.membersService.setUserParams(this.userParams);
      this.loadMembers();
    }
  }
}
