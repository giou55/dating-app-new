import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { map, of } from 'rxjs';
import { environment } from 'src/environments/environment';
import { Member } from '../_models/member';

@Injectable({
  providedIn: 'root'
})
export class MembersService {
  baseUrl = environment.apiUrl;

  // we store members here, so they will be available to every component
  // without making a new request to database 
  members: Member[] = [];

  constructor(private http:HttpClient) { }

  getMembers() {
    if (this.members.length > 0) return of(this.members);

    //return this.http.get<Member[]>(this.baseUrl + 'users', this.getHttpOptions())
    return this.http.get<Member[]>(this.baseUrl + 'users').pipe(
      map(members => {
        this.members = members;
        // here we return members because our components are using the members 
        return members;
      })
    )
  }

  getMember(username: string) {
    const member = this.members.find(x => x.userName === username);

    // if member exists inside members array, return the member without making a request
    if (member) return of(member);

    //return this.http.get<Member>(this.baseUrl + 'users/' + username, this.getHttpOptions())
    return this.http.get<Member>(this.baseUrl + 'users/' + username);
  }

  // we do not need this because we use jwt.interceptor to add Authorization header
  // getHttpOptions( ){
  //   const userString = localStorage.getItem('user');
  //   if (!userString) return;
  //   const user = JSON.parse(userString);
  //   return {
  //     headers: new HttpHeaders({
  //       Authorization: 'Bearer ' + user.token
  //     })
  //   }
  // }

  updateMember(member: Member) {
    return this.http.put(this.baseUrl + 'users', member).pipe(
      // nothing to return
      map(() => {
        const index = this.members.indexOf(member);
        // we update this specific member inside members array
        this.members[index] = {...this.members[index], ...member};
      })
    )
  }

  setMainPhoto(photoId: number) {
    return this.http.put(this.baseUrl + 'users/set-main-photo/' + photoId, {});
  }

  deletePhoto(photoId: number) {
    return this.http.delete(this.baseUrl + 'users/delete-photo/' + photoId);
  }
}
