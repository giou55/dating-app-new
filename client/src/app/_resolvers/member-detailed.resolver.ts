import { Injectable } from '@angular/core';
import {
  Router,
  Resolve,
  RouterStateSnapshot,
  ActivatedRouteSnapshot,
} from '@angular/router';
import { Observable, of } from 'rxjs';
import { Member } from '../_models/member';
import { MembersService } from '../_services/members.service';

// just like a service, this is injectable 
@Injectable({
  // it means that it's initialized when our app first starts
  providedIn: 'root',
})

// resolver is going to allow us to get the data before our route is activated,
// so we have our data before our component is constructed,
// and at this case the data is our member
export class MemberDetailedResolver implements Resolve<Member> {
  constructor(private membersService: MembersService) {}

  resolve(route: ActivatedRouteSnapshot): Observable<Member> {
    // we've got a complaint because the username could potentially be null,
    // so we add exclamation mark as a non-null assertion operator
    return this.membersService.getMember(route.paramMap.get('username')!);
  }
}
